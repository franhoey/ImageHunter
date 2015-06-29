using System;

namespace ImageHunter.Logging
{
    public class LoggingException : HunterException
    {
        public LoggingException()
        {
        }

        public LoggingException(string message)
            : base(message)
        {
        }

        public LoggingException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}