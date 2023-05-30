using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Threading;

namespace Trend2.Telegram.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var ev = new AsyncAutoResetEvent();

            var collector = new TgCollector(Configuration);

            collector.ChannelListStarted += (s, ea) =>
            {
                System.Console.WriteLine("Начат перебор списка каналов.");
            };

            collector.ChannelListCompleted += (s, ea) =>
            {
                System.Console.WriteLine("Перебор списка каналов завершен.");
            };

            collector.PostsBatchRead += (s, ea) =>
            {
                System.Console.WriteLine($" >>> {ea.Count} новых сообщений");
            };

            collector.ChannelStarted += (s, ea) => {
                System.Console.WriteLine(string.Empty);
                System.Console.WriteLine($"Начало обработки канала <{ea.Channel.Title}>(@{ea.Channel.Site} | {ea.Channel.Id})");
            };

            collector.ChannelSuccess += (s, ea) => {
                System.Console.WriteLine($"Канал <{ea.Channel.Title}>(@{ea.Channel.Site} | {ea.Channel.Id}) обработан.");
                System.Console.WriteLine($"Прочитано {ea.Posts} сообщений.");
                System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            };

            collector.ChannelError += (s, ea) => {
                System.Console.WriteLine($"Ошибка в процессе обработки канала <{ea.Channel.Title}>(@{ea.Channel.Site} | {ea.Channel.Id}):");
                System.Console.WriteLine(ea.Exception);
            };

            collector.Started += (s, ea) =>
            {
                System.Console.WriteLine("Запущен процесс автоматического сбора.");
            };

            collector.Stopped += (s, ea) =>
            {
                if (ea.Exception != null)
                {
                    System.Console.WriteLine("Процесс прерван по причине ошибки:");
                    System.Console.WriteLine(ea.Exception);
                }
                else
                {
                    System.Console.WriteLine("Процесс сбора штатно остановлен.");
                }
                System.Console.WriteLine(string.Empty);
                ev.Set();
            };
            collector.LoginError += (s, ea) =>
            {
                System.Console.WriteLine("Ошибка авторизации.");
                ev.Set();
            };
            collector.VerificationCodeRequested += (s, ea) =>
            {
                _ = Task.Delay(500).ContinueWith(_ =>
                {
                    System.Console.Write($"На телефонный номер {collector.PhoneNumber} был отправлен код. Введите его: ");
                    _ = collector.SetVerificationCodeAsync(System.Console.ReadLine() ?? string.Empty);
                }, TaskScheduler.Default);
            };
            collector.PhoneNumberRequested += (s, ea) =>
            {
                _ = Task.Delay(500).ContinueWith(_ =>
                {
                    System.Console.Write($"Введите номер телефона для подключения к Телеграм: ");
                    _ = collector.SetPhoneNumberAsync(System.Console.ReadLine() ?? string.Empty);
                }, TaskScheduler.Default);
            };

            await collector.StartAsync();

            var keyTask = Task.Run(() => System.Console.ReadKey(true));

            var t = await Task.WhenAny(keyTask, ev.WaitAsync());

            if (ReferenceEquals(t, keyTask))
            {
                await collector.StopAsync();

                System.Console.WriteLine("Нажмите любую кнопку для выхода...");
                System.Console.ReadKey(true);
            }
            else
            {
                System.Console.WriteLine("Нажмите любую кнопку для выхода...");
                await keyTask;
            }
        }
    }
}