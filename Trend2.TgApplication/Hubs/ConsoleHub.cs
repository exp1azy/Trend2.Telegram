using Microsoft.AspNetCore.SignalR;

namespace Trend2.TgApplication.Hubs
{
    /// <summary>
    /// Хаб для консоли.
    /// </summary>
    public class ConsoleHub : Hub
    {
        /// <summary>
        /// Метод SignalR для отправки сообщений на клиент о прогрессе скачивания.
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Сообщение содержит null</exception>
        public async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException();
            }

            await Clients.Caller.SendAsync("Receive", message);
        }
    }
}
