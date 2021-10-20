using System;
using System.Linq;
using VideoLibrary;
using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Services
{
    public class YoutubeService : IYoutubeService
    {
        private readonly YouTube _youtube;

        public YoutubeService()
        {
            _youtube = YouTube.Default;
        }

        public FetchVideoResult FetchVideos(string uri, bool includeNoAudio = false)
        {
            var args = new FetchVideoResult();

            try
            {
                var videos = includeNoAudio
                ? _youtube.GetAllVideos(uri)
                : _youtube.GetAllVideos(uri).Where(v => v.AudioFormat != AudioFormat.Unknown);

                var info = videos?.First().Info;

                args.Videos = videos.Where(v => v.AdaptiveKind is not AdaptiveKind.None && v.AudioFormat is not AudioFormat.Opus or AudioFormat.Vorbis).Select(v => new VideoWrapper(v)); ;
                args.VideoTitle = info.Title;
                args.VideoAuthor = info.Author;
                args.VideoLength = info.LengthSeconds;
                args.Success = true;
            }
            catch (Exception ex)
            {
                args.Success = false;
                args.Exception = ex;
            }

            return args;
        }

    }
}
