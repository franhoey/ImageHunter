namespace ImageHunter.Logging
{
    public interface IResultLogger
    {
        void OpenLogFile();
        void CloseLogFiles();
        void LogImage(SearchItem image);
    }
}