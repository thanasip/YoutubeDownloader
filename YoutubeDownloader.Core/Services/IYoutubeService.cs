using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Services
{
    public interface IYoutubeService
    {
        public FetchVideoResult FetchVideos(string uri, bool includeNoAudio = false);
    }
}
