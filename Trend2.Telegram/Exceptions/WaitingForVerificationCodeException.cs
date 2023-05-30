using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trend2.Telegram.Exceptions
{
    public class WaitingForVerificationCodeException : Exception
    {
        public WaitingForVerificationCodeException()
        {
        }

        public WaitingForVerificationCodeException(string? message) : base(message)
        {
        }

        public WaitingForVerificationCodeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected WaitingForVerificationCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}