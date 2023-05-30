using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using TL;
using Trend2.Telegram.Data;
using Trend2.Telegram.EventArgs;
using Trend2.Telegram.Exceptions;
using WTelegram;

namespace Trend2.Telegram
{
    /// <summary>
    /// Компонент, управляющий процессом сбора сообщений.
    /// </summary>
    public class TgCollector
    {
        private readonly IConfiguration _config;

        public TgCollector(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// true, если скачивание запущено, иначе, false.
        /// </summary>
        public bool IsStarted { get; private set; } = false;

        /// <summary>
        /// Номер телефона, через который выполнено подключение к Телеграм.
        /// </summary>
        public string PhoneNumber { get; protected set; }

        protected Exception? Exception { get; set; }
        protected CancellationTokenSource? CancellationTokenSource { get; set; }
        protected Task? Task { get; set; }
        protected Client? TgClient { get; set; }
        protected Entities.Channel? CurrentChannel { get; set; }
        protected AutoResetEvent? VerificationCodeReady { get; set; }
        protected string VerificationCode { get; set; }
        protected AutoResetEvent? PhoneNumberReady { get; set; }
        protected bool SingleChannelHandling { get; set; }

        private async Task DoAsync(CancellationToken cancellationToken, int startChannelId = 0)
        {
            try
            {
                Random random = new();
                var db = new DataContext(_config);

                bool includeStartChanId = true;

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var query = await GetChannelListAsync(db, cancellationToken);

                    if (ChannelFilterFunc != null)
                        query = query.Where(ChannelFilterFunc);

                    if (includeStartChanId)
                    {
                        includeStartChanId = false;
                        query = query.Where(s => s.Id >= startChannelId);
                    }

                    ChannelListStarted?.Invoke(this, new ChannelListStartedEventArgs { Count = query.Count() });

                    foreach (var channel in query)
                    {
                        await ChannelProcessingAsync(db, channel, cancellationToken);
                        await Task.Delay(random.Next(2000, 7000), cancellationToken);
                    }

                    ChannelListCompleted?.Invoke(this, new ChannelListCompletedEventArgs { Count = query.Count() });

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                ;
            }
            catch (Exception e)
            {
                Exception = e;
                _ = StopAsync();
            }
        }

        private async Task<IEnumerable<Entities.Channel>> GetChannelListAsync(DataContext db, CancellationToken cancellationToken)
        {
            List<SourceDao>? sources = null;
            int atempts = 0;

            do
            {
                try
                {
                    using var t = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted, cancellationToken);

                    sources = await db.Sources!.OrderBy(s => s.Id)
                        .Where(s => s.Type == "tg")
                        .Where(s => s.Enabled)
                        .ToListAsync(cancellationToken);

                    await t.CommitAsync(cancellationToken);
                }
                catch (Exception)
                {
                    sources = null;
                    atempts++;
                    if (atempts >= 5)
                        throw;

                    await Task.Delay(TimeSpan.FromSeconds(100), cancellationToken);
                }
            }
            while (sources == null);

            return sources!.Select(Entities.Channel.Map);
        }
 
        private async Task ChannelProcessingAsync(DataContext db, Entities.Channel channel, CancellationToken cancellationToken)
        {
            var regexLetters = new Regex("\\p{L}");
            var regexLink = new Regex("^(https?:\\/\\/)?([\\w-]{1,32}\\.[\\w-]{1,32})[^\\s@]*$");

            var count = 0;

            try
            {
                if (channel == null)
                    ChannelError?.Invoke(this, new ChannelErrorEventArgs { Channel = channel, Exception = new Exception() });

                ChannelStarted?.Invoke(this, new ChannelStartedEventArgs { Channel = channel });

                CurrentChannel = channel;

                var chat = await TgClient.Contacts_ResolveUsername(channel.Site);
                var peer = chat.Chat.ToInputPeer();

                var lastArticleId = await db.Articles.Where(a => a.SourceId == channel.Id)
                    .OrderByDescending(s => s.PubDate)
                    .Select(s => s.Link).FirstOrDefaultAsync(cancellationToken);

                _ = int.TryParse(lastArticleId, out var articleId);

                var pageSize = 100;
                var offset = 0;
                Messages_MessagesBase? messages;
                Random random = new();

                do
                {                 
                    messages = await TgClient.Messages_GetHistory(peer, min_id: articleId, add_offset: offset, limit: pageSize);
                    PostsBatchRead?.Invoke(this, new PostsBatchReadEventArgs { Channel = CurrentChannel, Count = messages.Messages.Length });

                    using (var trans = await db.Database.BeginTransactionAsync(cancellationToken))
                    {
                        foreach (var message in messages.Messages)
                        {
                            if (message is Message msg && regexLetters.Match(msg.message).Success && !regexLink.Match(msg.message).Success)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                    break;

                                string truncatedMessage = msg.message;
                                if (msg.message.Length > 50)
                                    truncatedMessage = msg.message[..50];

                                var index = truncatedMessage.LastIndexOfAny(new char[] { ',', '.', '!', '?', ':', ';', ' ' });
                                if (index >= 0)
                                {
                                    truncatedMessage = truncatedMessage[..index];
                                }

                                StringBuilder body = new ($"{msg.message}");
                                if (msg.media is MessageMediaWebPage media)
                                {
                                    if (media.webpage is WebPage webPage && webPage.description != null)
                                    {
                                        body.Append($"\n\n{webPage.description.ToString()}");
                                    }          
                                }

                                await db.Articles.AddAsync(new ArticleDao
                                {
                                    Title = Regex.Replace(truncatedMessage, "[\\p{Z}\\p{Zs}\\p{Zp}]+", " "),
                                    PubDate = msg.Date,
                                    Created = DateTime.UtcNow,
                                    Updated = DateTime.UtcNow,
                                    SourceId = channel.Id,
                                    Body = body.ToString(),
                                    Link = msg.id.ToString()
                                }, cancellationToken);

                                var chanCount = await db.Sources.Where(s => s.Id == channel.Id).FirstOrDefaultAsync(cancellationToken);
                                chanCount.Count ++;
                                db.Update(chanCount);
                            }
                        }

                        count += await db.SaveChangesAsync(cancellationToken);
                        await trans.CommitAsync(cancellationToken);
                    }

                    offset += pageSize;
                    await Task.Delay(random.Next(2000, 7000), cancellationToken);
                }
                while ((messages?.Messages?.Length ?? 0) > 0);
            }
            catch (OperationCanceledException)
            {
                ;
            } 
            catch (Exception ex)
            {
                ChannelError?.Invoke(this, new ChannelErrorEventArgs { Channel = channel, Exception = ex });
                return;
            }

            ChannelSuccess?.Invoke(this, new ChannelSuccessEventArg { Channel = channel, Posts = count });
        }

        private string Config(string key)
        {
            try
            {
                switch (key)
                {
                    case "api_id": return _config["Telegram:Id"];
                    case "api_hash": return _config["Telegram:Hash"];
                    case "phone_number": return GetPhoneNumber();
                    case "verification_code": return GetVerificationCode();
                    default: return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private string GetPhoneNumber()
        {
            PhoneNumberReady = new AutoResetEvent(false);
            PhoneNumberRequested?.Invoke(this, new System.EventArgs());

            PhoneNumberReady.WaitOne();
            PhoneNumberReady.Dispose();
            PhoneNumberReady = null;
            return PhoneNumber;
        }

        private string GetVerificationCode()
        {
            VerificationCodeReady = new AutoResetEvent(false);
            VerificationCodeRequested?.Invoke(this, new System.EventArgs());

            VerificationCodeReady.WaitOne();
            VerificationCodeReady.Dispose();
            VerificationCodeReady = null;
            return VerificationCode;
        }

        /// <summary>
        /// Метод установки кода верификации.
        /// </summary>
        /// <param name="verificationCode">Код верификации.</param>
        public async Task SetVerificationCodeAsync(string verificationCode)
        {
            if (VerificationCodeReady == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(verificationCode))
            {
                await TgClient.Auth_LogOut();
                return;
            }

            VerificationCode = verificationCode;
            VerificationCodeReady.Set();
        }

        /// <summary>
        /// Метод установки номера телефона.
        /// </summary>
        /// <param name="phoneNumber">Номер телефона.</param>
        public async Task SetPhoneNumberAsync(string phoneNumber)
        {
            if (PhoneNumberReady == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                await TgClient.Auth_LogOut();
                return;
            }

            PhoneNumber = phoneNumber;
            PhoneNumberReady.Set();
        }

        /// <summary>
        /// Метод запуска процесса автоматической сборки.
        /// </summary>
        /// <param name="startChannelId">Идентификатор канала, с которого начинать процесс сбора.</param>
        public async Task StartAsync(int startChannelId = 0)
        {
            if (Task != null || SingleChannelHandling)
            {
                throw new AlreadyStartedException();
            }
            if (VerificationCodeReady != null)
            {
                throw new WaitingForVerificationCodeException();
            }

            CancellationTokenSource = new CancellationTokenSource();

            try
            {
                TgClient = new Client(Config)
                {
                    MaxCodePwdAttempts = 1
                };
                await TgClient.LoginUserIfNeeded();                
            }
            catch (Exception ex)
            {
                if (TgClient != null)
                {
                    _ = await TgClient.Auth_LogOut();
                    TgClient?.Dispose();
                    TgClient = null;
                }
                VerificationCodeReady = null;                
                LoginError?.Invoke(this, ex);
                return;
            }

            Started?.Invoke(this, new System.EventArgs());
            IsStarted = true;

            Task = DoAsync(CancellationTokenSource.Token, startChannelId);
        }

        /// <summary>
        /// Метод остановки процесса автоматической сборки.
        /// </summary>
        public async Task StopAsync()
        {
            if (Task == null)
            {
                return;
            }
            if (VerificationCodeReady != null)
            {
                return;
            }

            CancellationTokenSource?.Cancel();

            await Task;

            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;

            Stopped?.Invoke(this, new StoppedEventArgs { Channel = CurrentChannel, Exception = Exception });

            Exception = null;
            CurrentChannel = null;
            Task = null;

            TgClient?.Dispose();
            TgClient = null;

            IsStarted = false;
        }

        /// <summary>
        /// Метод выполняет сбор сообщений с указанного канала
        /// </summary>
        /// <param name="channelId">Идентификатор канала.</param>
        /// <param name="cancellationToken">Токен отменты.</param>
        /// <returns>Задача, представляющая асинхронную операцию сбора сообщений.</returns>
        public async Task CollectChannelAsync(int channelId, CancellationToken cancellationToken = default)
        {
            if (Task != null)
            {
                throw new AlreadyStartedException();
            }
            if (VerificationCodeReady != null)
            {
                throw new WaitingForVerificationCodeException();
            }

            SingleChannelHandling = true;

            var db = new DataContext(_config);
            var channel = db.Sources.Where(c => c.Id == channelId).Select(Entities.Channel.Map).FirstOrDefault();

            TgClient = new Client(Config);
            await TgClient.LoginUserIfNeeded();

            await ChannelProcessingAsync(db, channel, cancellationToken);

            TgClient?.Dispose();
            TgClient = null;

            SingleChannelHandling = false;
        }

        /// <summary>
        /// Функция фильтрации каналов, которые должны участвовать в процессе автоматической сборки.
        /// </summary>
        public Func<Entities.Channel, bool>? ChannelFilterFunc { get; set; }

        /// <summary>
        /// Событие сигнализирует о начале автоматической сборки сообщений.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Событие сигнализирует об окончании автоматической сборки сообщений.
        /// </summary>
        public event EventHandler<StoppedEventArgs> Stopped;

        /// <summary>
        /// Событие сигнализирует о начале сборки сообщений с отдельного канала.
        /// </summary>
        public event EventHandler<ChannelStartedEventArgs> ChannelStarted;

        /// <summary>
        /// Событие сигнализирует о успешном завершении сборки сообщений с отдельного канала.
        /// </summary>
        public event EventHandler<ChannelSuccessEventArg> ChannelSuccess;

        /// <summary>
        /// Событие сигнализирует о аварийном завершении сборки сообщений с отдельного канала.
        /// </summary>
        public event EventHandler<ChannelErrorEventArgs> ChannelError;

        /// <summary>
        /// Событие, сигнализирующее о попытке чтения из канала новых сообщений.
        /// </summary>
        public event EventHandler<PostsBatchReadEventArgs> PostsBatchRead;

        /// <summary>
        /// Событие начала обработки списка каналов.
        /// </summary>
        public event EventHandler<ChannelListCompletedEventArgs> ChannelListCompleted;

        /// <summary>
        /// Событие окончания обработки списка каналов.
        /// </summary>
        public event EventHandler<ChannelListStartedEventArgs> ChannelListStarted;

        /// <summary>
        /// Событие, сигнализирующее, что коллектор запрашивает код подтверждения для подключения к Телеграм.
        /// </summary>
        public event EventHandler VerificationCodeRequested;

        /// <summary>
        /// Событие, сигнализирующее, что коллектор запрашивает номер телефона для подключения к Телеграм.
        /// </summary>
        public event EventHandler PhoneNumberRequested;

        /// <summary>
        /// Событие, сигнализирующее о том, что произошло ошибка в процессе авторизации
        /// </summary>
        public event EventHandler<Exception> LoginError;
    }
}