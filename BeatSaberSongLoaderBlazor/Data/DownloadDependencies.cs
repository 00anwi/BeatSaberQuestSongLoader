using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Data
{
    public class DownloadDependencies
    {
        public int progressBarFile;
        public int progressBarAll;
        public async Task DownloadAllDependencies()
        {

        }

        private async Task DownloadDependencie(string URL, string folder)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (s, e) =>
            {
                progressBarFile = e.ProgressPercentage;
            };
            webClient.DownloadFileCompleted += (s, e) =>
            {
                progressBarFile = 100;
                // any other code to process the file
            };
            webClient.DownloadFileAsync(new Uri(URL), folder);
        }
    }
}
