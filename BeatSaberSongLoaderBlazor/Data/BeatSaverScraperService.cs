using BeatSaberSongLoaderBlazor.Models;
using cloudscribe.HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Data
{
    public class BeatSaverScraperService
    {
        private readonly IOptions<FolderSettings> _config;

        public BeatSaverScraperService(IOptions<FolderSettings> config = null)
        {
            _config = config;
        }


        public List<BeatSaverSong> BeatSaverSongsList = new List<BeatSaverSong>();
        private string SongsLoadedURL;
        private int CurrentPage = 0;
        public string ProgressBar = "0";

        public async Task Search(string SearchString)
        {
            CurrentPage = 0;
            var URL = $"https://beatsaver.com/search/all/0?key={SearchString}";
            BeatSaverSongsList.Clear();
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(URL);
            var pageContents = await response.Content.ReadAsStringAsync();
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            LoadSongsFromBeatSaver(pageDocument);
            SongsLoadedURL = URL;
        }

        public async Task GetSongsFromURL(string URL)
        {
            CurrentPage = 0;
            BeatSaverSongsList.Clear();
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(URL);
            client.Dispose();
            var pageContents = await response.Content.ReadAsStringAsync();
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            LoadSongsFromBeatSaver(pageDocument);

            SongsLoadedURL = URL;
        }

        
        public async Task LoadMoreSongs()
        {
            CurrentPage += 1;
            var URL = $"{SongsLoadedURL}/{(CurrentPage * 20).ToString()}";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(URL);
            client.Dispose();
            var pageContents = await response.Content.ReadAsStringAsync();
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            LoadSongsFromBeatSaver(pageDocument);
        }

        private void LoadSongsFromBeatSaver(HtmlDocument pageDocument)
        {
            try
            {
                var SongList = pageDocument.DocumentNode.SelectNodes("(//div[contains(@style,'margin-bottom: 15px;')])");

                foreach (var song in SongList)
                {
                    string SongName = "", PictureSrc = "", BeatSaverUrl = "", DownloadLink = "", Downloads = "";

                    var HeaderNode = song.Descendants("h2").FirstOrDefault();
                    if (HeaderNode != null)
                    {
                        SongName = WebUtility.HtmlDecode(HeaderNode.InnerText.Trim());
                        BeatSaverUrl = HeaderNode.Descendants("a").FirstOrDefault().GetAttributeValue("href", "null");
                    }

                    var PictureSrcNode = song.Descendants("img").FirstOrDefault();
                    if (PictureSrcNode != null)
                    {
                        PictureSrc = PictureSrcNode.GetAttributeValue("src", "null");
                    }

                    var DownloadLinkNode = song.Descendants("a").Where(d => d.InnerText == "Download Zip").FirstOrDefault();
                    if (DownloadLinkNode != null)
                    {
                        DownloadLink = DownloadLinkNode.GetAttributeValue("href", "null");
                    }

                    var AddBeatSaverSong = new BeatSaverSong
                    {
                        PictureSrc = PictureSrc,
                        SongName = SongName,
                        BeatSaverUrl = BeatSaverUrl,
                        DownloadLink = DownloadLink
                    };

                    BeatSaverSongsList.Add(AddBeatSaverSong);
                }
            }
            catch
            {

            }

        }

        public class BeatSaverSong
        {
            public string PictureSrc { get; set; }
            public string SongName { get; set; }
            public string BeatSaverUrl { get; set; }
            public string DownloadLink { get; set; }
            public string Downloads { get; set; }
        }

        public async Task DownloadSong(string DownloadLink)
        {
            var BeatSaverSong = BeatSaverSongsList.Where(d => d.DownloadLink == DownloadLink).ToList();

            await Task.Run(() => DownloadBackground(BeatSaverSong));
        }

        public async Task DownloadAllSongs()
        {
            await Task.Run(() => DownloadBackground(BeatSaverSongsList));
        }

        private void DownloadBackground(List<BeatSaverSong> beatSaverSongs)
        {
            var Tempfolder = ".\\temp";
            // If the directory doesn't exist, create it.
            if (!Directory.Exists(Tempfolder))
            {
                Directory.CreateDirectory(Tempfolder);
            }

            double updateProgressAmountForeachSong = 0;
            double tempProgress = 0;
            ProgressBar = "0";
            ProgressBarUpdatedEvent?.Invoke(this, new ProgressBarUpdatedEventArgs(ProgressBar));

            if (beatSaverSongs.Any())
            {
                updateProgressAmountForeachSong = 100.0 / beatSaverSongs.Count();
            }

            foreach (var song in beatSaverSongs)
            {
                var filePath = Path.Combine(Tempfolder, Guid.NewGuid().ToString() + ".zip");

                var wc = new WebClient();
                wc.DownloadFile(song.DownloadLink, filePath);

                try
                {
                    ZipFile.ExtractToDirectory(filePath, _config.Value.AvailableSongsFolder);
                }
                catch
                {

                }

                File.Delete(filePath);

                tempProgress += updateProgressAmountForeachSong;

                int holeNumber = (int)tempProgress;

                if (holeNumber > 98)
                {
                    holeNumber = 100;
                }

                ProgressBar = holeNumber.ToString();

                ProgressBarUpdatedEvent?.Invoke(this, new ProgressBarUpdatedEventArgs(ProgressBar));
            }
        }

        public delegate void ProgressBarUpdatedEventHandler(object sender, ProgressBarUpdatedEventArgs e);

        public event ProgressBarUpdatedEventHandler ProgressBarUpdatedEvent;

        public class ProgressBarUpdatedEventArgs : EventArgs
        {
            public ProgressBarUpdatedEventArgs(string progressBar)
            {
                ProgressBar = progressBar;
            }

            public string ProgressBar { get; set; }
        }
    }
}
