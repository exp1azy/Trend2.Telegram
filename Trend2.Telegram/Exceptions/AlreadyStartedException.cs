using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trend2.Telegram.Exceptions
{
    public class AlreadyStartedException : Exception
    {
        public AlreadyStartedException() : base() { } 

        public AlreadyStartedException(string? message) : base(message) { }

        public AlreadyStartedException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
