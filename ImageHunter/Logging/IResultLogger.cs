namespace ImageHunter.Logging
{
    public interface IResultLogger
    {
        void OpenLogFile();
        void CloseLogFile();
        void LogImage(FoundImage image);
    }
}