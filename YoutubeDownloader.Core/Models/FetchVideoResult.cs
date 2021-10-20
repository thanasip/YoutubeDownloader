using System;
using System.Collections.Generic;
using VideoLibrary;

namespace YoutubeDownloader.Core.Models
{
    public class FetchVideoResult
    {
        public IEnumerable<VideoWrapper> Videos { get; set; }

        public string VideoTitle { get; set; }

        public string VideoAuthor { get; set; }

        public int? VideoLength { get; set; }

        public bool Success { get; set; }

        public Exception Exception { get; set; }
    }
}
