using Trend2.Telegram.Entities;

namespace Trend2.Telegram.EventArgs
{
    /// <summary>
    /// Параметры события остановки процесса автоматической сборки.
    /// </summary>
    public class StoppedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Информация о канале, на котором произошла остановка.
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// Исключение, ставшее причиной остановки. Если остановка произошла в штатном режиме - null.
        /// </summary>
        public Exception? Exception { get; set; }
    }
}