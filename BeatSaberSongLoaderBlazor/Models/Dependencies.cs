using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Models
{
    public class Dependencies
    {
        public IEnumerable<Dependencie> Dependencie { get; set; }
    }

    public class Dependencie
    {
        public string Name { get; set; }
        public string DownloadURLWindows { get; set; }
        public string DownloadURLLinux { get; set; }
        public string DownloadURLOSX { get; set; }
        public string Folder { get; set; }
    }
}
