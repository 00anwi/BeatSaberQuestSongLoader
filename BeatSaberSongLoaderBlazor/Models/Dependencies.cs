using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Models
{
    public class Dependencies
    {
        public List<Dependencie> dependencies { get; set; }
    }

    public class Dependencie
    {
        public string DependencieName { get; set; }
        public DependencieSettings DependencieSettings { get; set; }
    }

    public class DependencieSettings
    {
        public string DownloadURLWindows { get; set; }
        public string DownlaodURLLinux { get; set; }
        public string DownloadURLMAC { get; set; }
        public string Folder { get; set; }
    }
}
