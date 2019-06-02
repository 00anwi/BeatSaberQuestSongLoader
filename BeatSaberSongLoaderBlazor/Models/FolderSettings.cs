using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Models
{
    public class FolderSettings
    {
        public string AvailableSongsFolder { get; set; }
        public string SongsToLoadFolder { get; set; }
        public string apkFolder { get; set; }
        public string assetsFolder { get; set; }
        public string backupFolder { get; set; }
        public string BeatMapAssetMaker_Folder { get; set; }
        public string songeconverter_Folder { get; set; }
        public string apktool_Folder { get; set; }
        public string ADB_Folder { get; set; }
        public string JARSIGNERLOC_Folder { get; set; }
        public bool UseToolsFolder { get; set; }
        public string toolsFolder { get; set; }
        public string uber_apk_signer_Folder { get; set; }
        
    }
}
