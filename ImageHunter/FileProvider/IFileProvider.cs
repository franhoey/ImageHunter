using System.Collections;
using System.Collections.Generic;

namespace ImageHunter.FileProvider
{
    public interface IFileProvider
    {
        IEnumerable<SearchItem> GetFilePaths();
        SearchItem GetFile(SearchItem path);
    }
}