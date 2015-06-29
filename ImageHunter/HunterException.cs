using System;

namespace ImageHunter
{
    public class HunterException : ApplicationException
    {
        public HunterException()
        {
        }

        public HunterException(string message)
            : base(message)
        {
        }

        public HunterException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}