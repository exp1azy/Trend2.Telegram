using Trend2.Telegram.Entities;

namespace Trend2.Telegram.EventArgs
{
    /// <summary>
    /// Параметры события аварийного окончания процесса сбора сообщений с отдельного канала.
    /// </summary>
    public class ChannelErrorEventArgs : System.EventArgs
    {
        /// <summary>
        /// Информация о канале.
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// Исключение, ставшее причиной аварийной остановки.
        /// </summary>
        public Exception Exception { get; set; }
    }
}