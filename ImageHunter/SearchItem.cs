using System;
using System.Runtime.Serialization;

namespace ImageHunter
{
    public class SearchItem
    {
        public SearchItem()
        {
            Status = Statuses.Ok;
        }

        public enum Statuses
       {
           Ok,
           Failed
       }

        public string FilePath { get; set; }
        public string FileContents { get; set; }
        public string ImageUrl { get; set; }
        public string ShortUrl { get; set; }

        public Statuses Status { get; set; }
        public HunterException Error { get; set; }


        public SearchItem Clone()
        {
            return new SearchItem()
            {
                FilePath = FilePath,
                FileContents = FileContents,
                ImageUrl = ImageUrl,
                ShortUrl = ShortUrl,
                Status = Status,
                Error = Error
            };
        }
    }
}