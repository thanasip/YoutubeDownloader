using System;

namespace YoutubeDownloader.Core.Settings
{
    public class AppSettings
    {
        public string BaseSaveDirectory { get; set; }

        public bool UseCustomPath { get; set; }

        public string CustomPath { get; set; }

        public string HomeDirectory => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public string SavePath => !UseCustomPath ? $"{HomeDirectory}\\{BaseSaveDirectory}" : CustomPath;
    }
}
