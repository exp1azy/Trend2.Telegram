using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Threading;
using System.Text.RegularExpressions;
using TL;
using Trend2.Telegram;
using Trend2.TgApplication.ViewModels;

namespace Trend2.TgApplication.Controllers
{
    /// <summary>
    /// Контроллер домашней страницы.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly TgCollector _tgCollector;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;

        private readonly ManualResetEvent startedEv = new(false);
        private readonly ManualResetEvent verificationEv = new(false);
        private readonly ManualResetEvent loginErrorEv = new(false);
        private readonly ManualResetEvent phoneNumberEv = new(false);

        private Exception loginException = null;

        public HomeController(TgCollector tgCollector, IConfiguration config, IMemoryCache memoryCache)
        {
            _config = config;
            _tgCollector = tgCollector;
            _memoryCache = memoryCache;
        }

        private void StartedHandler(object? sender, EventArgs e)
        {
            startedEv.Set();
        }

        private void VerificationHandler(object? sender, EventArgs e)
        {
            verificationEv.Set();
        }

        private void LoginErrorHandler(object? sender, Exception ex)
        {
            loginException = ex;
            loginErrorEv.Set();
        }

        private void PhoneNumberHandler(object? sender, EventArgs e)
        {
            phoneNumberEv.Set();
        }

        /// <summary>
        /// Метод загрузки домашней страницы.
        /// </summary>
        /// <returns>Домашняя страница.</returns>
        [HttpGet]
        public IActionResult HomePage()
        {
            return View(new VerificationViewModel{ IsStarted = _tgCollector.IsStarted, Number = _memoryCache.Get("Number")?.ToString() });
        }

        /// <summary>
        /// Метод для запуска скачивания постов из источников.
        /// </summary>
        /// <returns>
        /// Ok("phoneNumberRequested"), если запрашивается номер телефона;
        /// Ok("started"), если скачивание запущено;
        /// Ok("verificationRequested"), если запрашивается код верификации;
        /// Ok("error: .."), если произошла ошибка во время авторизации.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> StartDownload()
        {
            phoneNumberEv.Reset();
            loginErrorEv.Reset();

            _tgCollector.PhoneNumberRequested += PhoneNumberHandler;
            _tgCollector.LoginError += LoginErrorHandler;

            _ = _tgCollector.StartAsync();
        
            var phoneNumberTask = Task.Run(phoneNumberEv.WaitOne);
            var loginErrorTask = Task.Run(loginErrorEv.WaitOne);

            var endTask = await Task.WhenAny(phoneNumberTask, loginErrorTask);
            _tgCollector.PhoneNumberRequested -= PhoneNumberHandler;
            _tgCollector.LoginError -= LoginErrorHandler;

            if (ReferenceEquals(phoneNumberTask, endTask))
            {
                _memoryCache.TryGetValue("Number", out string? phoneNumber);

                if (phoneNumber == null)
                {
                    return Ok("phoneNumberRequested");
                }
                else
                {
                    startedEv.Reset();
                    verificationEv.Reset();
                    loginErrorEv.Reset();

                    _tgCollector.Started += StartedHandler;
                    _tgCollector.VerificationCodeRequested += VerificationHandler;
                    _tgCollector.LoginError += LoginErrorHandler;

                    await _tgCollector.SetPhoneNumberAsync(phoneNumber);

                    var verificationTask = Task.Run(verificationEv.WaitOne);
                    var startedTask = Task.Run(startedEv.WaitOne);
                    loginErrorTask = Task.Run(loginErrorEv.WaitOne);

                    endTask = await Task.WhenAny(startedTask, verificationTask, loginErrorTask);
                    _tgCollector.Started -= StartedHandler;
                    _tgCollector.VerificationCodeRequested -= VerificationHandler;
                    _tgCollector.LoginError -= LoginErrorHandler;

                    if (ReferenceEquals(startedTask, endTask))
                    {
                        return Ok("started");
                    }
                    else if (ReferenceEquals(verificationTask, endTask))
                    {
                        return Ok("verificationRequested");
                    }
                    else
                    {
                        return Ok($"error: {loginException?.Message ?? "null"}{(loginException is RpcException rpce ? $" - {rpce.X}" : string.Empty)}");
                    }
                }
            }
            else
            {
                return Ok($"error: {loginException?.Message ?? "null"}{(loginException is RpcException rpce ? $" - {rpce.X}" : string.Empty)}");
            }
        }

        /// <summary>
        /// Метод запуска скачивания с другого номера
        /// </summary>
        /// <returns>Метод запуска скачивания StartDownload().</returns>
        [HttpGet]
        public async Task<IActionResult> StartDownloadFromAnotherNumber()
        {
            _memoryCache.Remove("Number");

            return await StartDownload();
        }

        /// <summary>
        /// Метод для отображения формы ввода номера телефона.
        /// </summary>
        /// <returns>Форма ввода номера.</returns>
        [HttpGet]
        public IActionResult ShowPhoneNumberForm()
        {
            return View("PhoneNumberForm", new VerificationViewModel { Id = _config["Telegram:Id"] });
        }

        /// <summary>
        /// Форма ввода номера.
        /// </summary>
        /// <param name="phoneNumber">Номер телефона.</param>
        /// <returns>
        /// Если скачивание запущено, происходит переадресация на HomePage;
        /// Если запрашивается код верификации, происходит переадресация на ShowVerificationForm;
        /// Если произошла ошибка подключения к Телеграму или номер введен неверно, возвращается та же форма с сообщением об ошибке.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> PhoneNumberForm(string phoneNumber = "")
        {
            var regex = new Regex(@"^\+\d{11}$");

            if (regex.Match(phoneNumber).Success) 
            {
                _memoryCache.Set("Number", phoneNumber, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24)));

                startedEv.Reset();
                verificationEv.Reset();
                loginErrorEv.Reset();

                _tgCollector.Started += StartedHandler;
                _tgCollector.VerificationCodeRequested += VerificationHandler;
                _tgCollector.LoginError += LoginErrorHandler;

                _ = _tgCollector.SetPhoneNumberAsync(phoneNumber);

                var verificationTask = Task.Run(verificationEv.WaitOne);
                var startedTask = Task.Run(startedEv.WaitOne);
                var loginErrorTask = Task.Run(loginErrorEv.WaitOne);

                var endTask = await Task.WhenAny(startedTask, verificationTask, loginErrorTask);
                _tgCollector.Started -= StartedHandler;
                _tgCollector.VerificationCodeRequested -= VerificationHandler;
                _tgCollector.LoginError -= LoginErrorHandler;

                if (ReferenceEquals(startedTask, endTask))
                {
                    return RedirectToAction("HomePage");
                }
                else if (ReferenceEquals(verificationTask, endTask))
                {
                    return RedirectToAction("ShowVerificationForm");
                }
                else
                {
                    return View(new VerificationViewModel { Id = _config["Telegram:Id"], ErrorMessage = "Ошибка подключения к Телеграму" });
                }
            }
            else
            {
                return View(new VerificationViewModel { Id = _config["Telegram:Id"], ErrorMessage = "Ожидается номер в формате +XXXX" });
            }
        }

        /// <summary>
        /// Метод для отображения формы верификации.
        /// </summary>
        /// <returns>Форма верификации.</returns>
        [HttpGet]
        public IActionResult ShowVerificationForm()
        {            
            return View("Verification", new VerificationViewModel { Id = _config["Telegram:Id"], Number = _tgCollector.PhoneNumber });
        }

        /// <summary>
        /// Метод для верификации клиента.
        /// </summary>
        /// <param name="verificationCode">Код верификации</param>
        /// <returns>Возвращает домашнюю страницу, если верификация прошла успешно, иначе, форму для верификации.</returns>
        [HttpPost]
        public async Task<IActionResult> Verification(string verificationCode = "")
        {
            var regex = new Regex("^\\d{5}$");

            if (regex.Match(verificationCode).Success)
            {
                startedEv.Reset();
                loginErrorEv.Reset();

                _tgCollector.Started += StartedHandler;
                _tgCollector.LoginError += LoginErrorHandler;

                _ = _tgCollector.SetVerificationCodeAsync(verificationCode);

                var loginErrorTask = Task.Run(loginErrorEv.WaitOne);
                var startedTask = Task.Run(startedEv.WaitOne);

                var endTask = await Task.WhenAny(startedTask, loginErrorTask);
                _tgCollector.Started -= StartedHandler;
                _tgCollector.LoginError -= LoginErrorHandler;

                return RedirectToAction("HomePage");
            }
            else
            {
                return View(new VerificationViewModel { Id = _config["Telegram:Id"], Number = _tgCollector.PhoneNumber, ErrorMessage = "Ожидается пятизначный цифровой код подтверждения" });
            }
        }

        /// <summary>
        /// Метод для остановки скачивания постов из источников.
        /// </summary>
        /// <returns>Ok с параметром "stopped".</returns>
        [HttpGet]
        public async Task<IActionResult> StopDownload()
        {
            await _tgCollector.StopAsync();

            return Ok("stopped");
        }
    }
}