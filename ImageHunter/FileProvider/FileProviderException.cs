using System;

namespace ImageHunter.FileProvider
{
    public class FileProviderException : HunterException
    {
        public FileProviderException()
        {
        }

        public FileProviderException(string message)
            : base(message)
        {
        }

        public FileProviderException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}