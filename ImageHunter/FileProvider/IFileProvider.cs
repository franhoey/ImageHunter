using System.Collections;
using System.Collections.Generic;

namespace ImageHunter.FileProvider
{
    public interface IFileProvider
    {
        IEnumerable<string> GetFilePaths();
        SearchableFile GetFile(string path);
    }
}