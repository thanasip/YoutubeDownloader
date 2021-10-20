using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeDownloader.Core.Settings;
using YoutubeDownloader.Core.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Collections.ObjectModel;
using YoutubeDownloader.Core.Models;
using System.ComponentModel;
using System.Net.Http;
using Path = System.IO.Path;

namespace YoutubeDownloader.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly IYoutubeService _youtube;

        public MainWindow(IOptions<AppSettings> settings, IYoutubeService youtube)
        {
            InitializeComponent();
            _settings = settings.Value;
            _youtube = youtube;
        }

        private void FetchVideoBtn_Click(object sender, RoutedEventArgs e)
        {
            FetchVideoBtn.IsEnabled = false;
            SelectedItemCombo.ItemsSource = null;

            var worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            worker.DoWork += (sender, args) =>
            {
                args.Result = _youtube.FetchVideos((string)args.Argument);
                foreach (var v in (args.Result as FetchVideoResult).Videos)
                {
                    Debug.Print($"{v.Video.Resolution} - {v.Video.Uri}");
                }
            };

            worker.RunWorkerCompleted += (sender, args) =>
            {
                FetchVideoBtn.IsEnabled = true;
                DownloadVideoBtn.IsEnabled = true;
                SelectedItemCombo.ItemsSource = (args.Result as FetchVideoResult).Videos;
                SelectedItemCombo.SelectedIndex = 0;
            };

            worker.RunWorkerAsync(UriInput.Text);
        }

        private void SelectedItemCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = (e.AddedItems[0] as VideoWrapper);
                TitleLabel.Text = $"{item.Video.Title}";
                AuthorLabel.Text = $"{item.Video.Info.Author}";
                LengthLabel.Text = $"{TimeSpan.FromSeconds(item.Video.Info.LengthSeconds ?? 0):hh\\:mm\\:ss}";
                SizeLabel.Text = $"{item.ContentLengthMb} Mb";
                ResLabel.Text = $"{(item.Video.AdaptiveKind == VideoLibrary.AdaptiveKind.Video ? $"{item.Video.Resolution}p" : "N/A")}";
                VidFormatLabel.Text = $"{(item.Video.Format != VideoLibrary.VideoFormat.Unknown ? item.Video.Format : "N/A")}".ToUpper();
                AudFormatLabel.Text = $"{item.Video.AudioFormat}".ToUpper();
                AudRateLabel.Text = $"{item.Video.AudioBitrate}kbps";
            }
            catch (Exception ex)
            {
                // Probably just cleared the list
                Debug.WriteLine($"Whoops: {ex.Message}");

                TitleLabel.Text = null;
                AuthorLabel.Text = null;
                LengthLabel.Text = null;
                SizeLabel.Text = null;
                ResLabel.Text = null;
                VidFormatLabel.Text = null;
                AudFormatLabel.Text = null;
                AudRateLabel.Text = null;
            }
        }

        private void DownloadVideoBtn_Click(object sender, RoutedEventArgs e)
        {
            DownloadVideoBtn.IsEnabled = false;
            FetchVideoBtn.IsEnabled = false;

            var worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            worker.DoWork += DownloadVideo;

            worker.ProgressChanged += (sender, args) =>
            {
                ProgressComplete.Value = args.ProgressPercentage;
            };

            worker.RunWorkerCompleted += (sender, args) =>
            {
                DownloadVideoBtn.IsEnabled = true;
                FetchVideoBtn.IsEnabled = true;
                MessageBox.Show("Download complete!", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
                ProgressComplete.Value = 0;
            };

            worker.RunWorkerAsync(SelectedItemCombo.SelectedItem);
        }

        private void DownloadVideo(object sender, DoWorkEventArgs args)
        {
            var video = args.Argument as VideoWrapper;

            long totalByte = video.ContentLength;

            using(var client = new HttpClient())
            using (Stream output = File.OpenWrite($"{_settings.SavePath}\\{StripIllegalPathChars(video.Video.Title)}{video.Extension}"))
            {
                using (var input = client.GetStreamAsync(video.Video.Uri).GetAwaiter().GetResult())
                {
                    byte[] buffer = new byte[16 * 1024];
                    int read;
                    long totalRead = 0;
                    Debug.WriteLine("Download Started");
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                        totalRead += read;
                        (sender as BackgroundWorker).ReportProgress((int)((100 * totalRead) / totalByte));
                        Debug.Write($"\rDownloading {totalRead}/{totalByte} ...");
                    }
                    output.Flush();
                    output.Close();
                    Debug.WriteLine("Download Complete");
                }
            }
        }

        private string StripIllegalPathChars(string s)
        {
            var illegal = Path.GetInvalidFileNameChars();
            foreach(var c in illegal)
            {
                s = s.Replace(c, '_');
            }

            return s;
        }
    }
}
