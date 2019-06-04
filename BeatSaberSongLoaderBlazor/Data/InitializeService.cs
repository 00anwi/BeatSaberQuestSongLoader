using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BeatSaberSongLoaderBlazor.Models;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace BeatSaberSongLoaderBlazor.Data
{
    public class InitializeService
    {
        private readonly IOptions<FolderSettings> _FolderSettings;
        private readonly IOptions<Dependencies> _Dependencies;

        public InitializeService(IOptions<FolderSettings> FolderSettings = null,
            IOptions<Dependencies> Dependencies = null)
        {
            _FolderSettings = FolderSettings;
            _Dependencies = Dependencies;
        }

        public string ProgressBar = "0";

        public async Task GetAllDependencies()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await Task.Run(() => DownloadDependencies("Windows"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                await Task.Run(() => DownloadDependencies("Linux"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                await Task.Run(() => DownloadDependencies("OSX"));
            }
            else
            {
                // throw 
            }
        }

        private void DownloadDependencies(string OS)
        {
            var Tempfolder = ".\\temp";
            // If the directory doesn't exist, create it.
            if (!Directory.Exists(Tempfolder))
            {
                Directory.CreateDirectory(Tempfolder);
            }

            var Dependencies = _Dependencies.Value.Dependencie;
            

            double updateProgressAmountForeachDependencie = 0;
            double tempProgress = 0;
            ProgressBar = "0";
            ProgressBarUpdatedEvent?.Invoke(this, new ProgressBarUpdatedEventArgs(ProgressBar));

            if (Dependencies.Any())
            {
                updateProgressAmountForeachDependencie = 100.0 / Dependencies.Count();
            }

            foreach (var Dependencie in Dependencies)
            {
                var filePath = Path.Combine(Tempfolder, Guid.NewGuid().ToString() + ".zip");

                var wc = new WebClient();
                wc.DownloadProgressChanged += (s, e) =>
                {
                    ProgressBarDependencieUpdatedEvent?.Invoke(this, new ProgressBarDependencieUpdatedEventArgs(e.ProgressPercentage.ToString(), Dependencie.Name));
                };

                wc.DownloadFileCompleted += (s, e) =>
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(filePath, Dependencie.Folder);
                    }
                    catch
                    {

                    }

                    File.Delete(filePath);

                    tempProgress += updateProgressAmountForeachDependencie;

                    int holeNumber = (int)tempProgress;

                    if (holeNumber > 98)
                    {
                        holeNumber = 100;
                    }

                    ProgressBar = holeNumber.ToString();

                    ProgressBarUpdatedEvent?.Invoke(this, new ProgressBarUpdatedEventArgs(ProgressBar));
                };

                if (OS == "Windows")
                {
                    wc.DownloadFileAsync(new Uri(Dependencie.DownloadURLWindows), filePath);
                }
                else if(OS == "Linux")
                {
                    wc.DownloadFileAsync(new Uri(Dependencie.DownloadURLLinux), filePath);
                }
                else if (OS == "OSX")
                {
                    wc.DownloadFileAsync(new Uri(Dependencie.DownloadURLOSX), filePath);
                }

                while (wc.IsBusy)
                {

                }
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


        public delegate void ProgressBarDependencieUpdatedEventHandler(object sender, ProgressBarDependencieUpdatedEventArgs e);

        public event ProgressBarDependencieUpdatedEventHandler ProgressBarDependencieUpdatedEvent;

        public class ProgressBarDependencieUpdatedEventArgs : EventArgs
        {
            public ProgressBarDependencieUpdatedEventArgs(string progressBar, string dependencieName)
            {
                ProgressBar = progressBar;
                DependecieName = dependencieName;
            }

            public string DependecieName { get; set; }
            public string ProgressBar { get; set; }
        }
    }
}
