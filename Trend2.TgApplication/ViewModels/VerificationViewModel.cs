namespace Trend2.TgApplication.ViewModels
{
    public class VerificationViewModel
    {
        /// <summary>
        /// Telegram API Идентификатор.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Номер телефона.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Код верификации.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// true, если скачивание запущено, иначе, false
        /// </summary>
        public bool IsStarted { get; set; }
    }
}