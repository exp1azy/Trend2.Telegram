using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trend2.Telegram.Entities;

namespace Trend2.Telegram.EventArgs
{
    /// <summary>
    /// Параметры события начала сбора сообщений с отдельного канала.
    /// </summary>
    public class ChannelStartedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Информация о канале.
        /// </summary>
        public Channel Channel { get; set; }
    }
}