using System;
using System.Net.Http;
using VideoLibrary;

namespace YoutubeDownloader.Core.Models
{
    public class VideoWrapper
    {
        public VideoWrapper(YouTubeVideo video)
        {
            _video = video;
            _contentLength = GetContentLength(video);
            _contentLengthMb = Math.Round(_contentLength / 1048576F, 2);

            _extension = video.AdaptiveKind == AdaptiveKind.Video 
                ? ".mp4" 
                : video.AudioFormat == AudioFormat.Aac 
                    ? ".m4a" 
                    : ".mp3";

            _description = $"{video.Title} ({TimeSpan.FromSeconds(video.Info.LengthSeconds ?? 0):hh\\:mm\\:ss})";
        }

        private YouTubeVideo _video;
        public YouTubeVideo Video
        {
            get => _video;
            set => _video = value;
        }

        private string _description;
        public string VideoDescription => _description;

        private double _contentLengthMb;
        public double ContentLengthMb
        {
            get => _contentLengthMb;
            set => _contentLengthMb = value;
        }

        private long _contentLength;
        public long ContentLength
        {
            get => _contentLength;
            set => _contentLength = value;
        }

        private string _extension;
        public string Extension
        {
            get => _extension;
            set => _extension = value;
        }

        private long GetContentLength(YouTubeVideo video)
        {
            
            var totalBytes = 0L;

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Head, video.Uri))
            {
                totalBytes = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength ?? 0L;
            }

            return totalBytes;
        }
    }
}
