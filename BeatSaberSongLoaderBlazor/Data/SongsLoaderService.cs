using BeatSaberSongLoaderBlazor.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Data
{
    public class SongsLoaderService
    {
        private readonly IOptions<FolderSettings> _config;

        public SongsLoaderService(IOptions<FolderSettings> config)
        {
            _config = config;
        }

        public List<SongData> AvailableSongs = new List<SongData>();
        public List<SongData> SongsToLoad = new List<SongData>();
        public string ProgressBar = "0";

        private List<SongData> AllAvailableSongs = new List<SongData>();
        private List<SongData> AllSongsToLoad = new List<SongData>();

        public void LoadSongs()
        {
            SongsToLoad.Clear();
            AvailableSongs.Clear();

            LoadAvailableSongs();
            LoadSongsToLoad();

            foreach (var song in AllSongsToLoad)
            {
                SongsToLoad.Add(song);
            }

            foreach (var song in AllAvailableSongs)
            {
                var anySongInSongsToLoad = SongsToLoad.Where(s => s.SongName == song.SongName);

                if (!anySongInSongsToLoad.Any())
                {
                    AvailableSongs.Add(song);
                }
            }
        }

        private void LoadAvailableSongs()
        {
            var path = _config.Value.AvailableSongsFolder;

            AllAvailableSongs = GetSongData(path);
        }

        private void LoadSongsToLoad()
        {
            var path = _config.Value.SongsToLoadFolder;

            AllSongsToLoad = GetSongData(path);
        }

        public void TransferSongsToQuestSongDir(string[] selectedSongs)
        {
            double updateProgressAmountForeachSong = 0;
            double tempProgress = 0;
            ProgressBar = "0";
            ProgressBarUpdatedEvent?.Invoke(this, new ProgressBarUpdatedEventArgs(ProgressBar));

            if (selectedSongs.Any())
            {
                updateProgressAmountForeachSong = 100.0 / selectedSongs.Count();
            }

            foreach (var song in selectedSongs)
            {
                var songName = song.ToString();
                var AvailableSong = AvailableSongs.Single(s => s.SongName == song.ToString());
                var isValid = !string.IsNullOrEmpty(songName) &&
                    songName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                    !File.Exists(Path.Combine(_config.Value.SongsToLoadFolder, songName));

                if (!isValid)
                {
                    songName = string.Join("", songName.Split(Path.GetInvalidFileNameChars()));
                }
                var destDirName = (_config.Value.SongsToLoadFolder + songName).Trim();
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(AvailableSong.SongFolder);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + AvailableSong.SongFolder);
                }
                else if(dir.FullName.Substring(dir.FullName.Length - 1, 1) == " ")
                {
                    Directory.Move(dir.FullName, dir.FullName.TrimEnd());
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName)) 
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }

                tempProgress += updateProgressAmountForeachSong;

                int holeNumber = (int)tempProgress;

                if(holeNumber > 98)
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



        private List<SongData> GetSongData(string path)
        {
            var songList = new List<SongData>();

            string[] Infofiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(f => f.Contains("info.dat") || f.Contains("info.json")).ToArray();

            foreach (var file in Infofiles)
            {
                var filepath = Directory.GetParent(file).FullName.ToString();
                string songName = null;

                if(Path.GetExtension(file).ToLowerInvariant() == ".dat")
                {
                    songName = ParseNewInfoFileAndReturnSongName(file, filepath);
                }
                else
                {
                    songName = ParseOldInfoFileAndReturnSongName(file, filepath);
                }

                if (!String.IsNullOrEmpty(songName))
                {
                    var anySong = songList.Where(s => s.SongName.Equals(songName, StringComparison.InvariantCultureIgnoreCase));

                    if (!anySong.Any())
                    {
                        var songData = new SongData
                        {
                            SongFolder = filepath,
                            SongName = songName
                        };
                        songList.Add(songData);
                    }
                }
            }

            return songList;
        }

        public class SongData
        {
            public string SongName { get; set; }
            public string SongFolder { get; set; }
        }

        private string ParseNewInfoFileAndReturnSongName(string filepath, string subdir)
        {
            string result = string.Empty;

            using (StreamReader r = new StreamReader(filepath))
            {
                var json = r.ReadToEnd();
                var jobj = JObject.Parse(json);
                foreach (var item in jobj.Properties())
                {
                    if (item.Name == "_songName")
                    {
                        return item.Value.ToString();
                    }
                }
            }

            return "";
        }

        private string ParseOldInfoFileAndReturnSongName(string filepath, string subdir)
        {
            string result = string.Empty;

            using (StreamReader r = new StreamReader(filepath))
            {

                var json = r.ReadToEnd();
                var jobj = JObject.Parse(json);
                foreach (var item in jobj.Properties())
                {
                    if (item.Name == "songName")
                    {
                        return item.Value.ToString();
                    }

                }
            }
            return "";
        }

        private bool FileExist(string FilePath)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return false;
            }
            else
            {
                if (File.Exists(FilePath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task LoadSongsToQuest()
        {
            var apkFolder = _config.Value.apkFolder;
            var ADB_Folder = _config.Value.ADB_Folder;
            var backupFolder = _config.Value.backupFolder;
            var apktool_Folder = _config.Value.apktool_Folder;
            var songeconverter_Folder = _config.Value.songeconverter_Folder;
            var SongsToLoadFolder = _config.Value.SongsToLoadFolder;
            var BeatMapAssetMaker_Folder = _config.Value.BeatMapAssetMaker_Folder;
            var JARSIGNERLOC_Folder = _config.Value.JARSIGNERLOC_Folder;
            var uber_apk_signer_Folder = _config.Value.uber_apk_signer_Folder;

            if (_config.Value.UseToolsFolder)
            {
                BeatMapAssetMaker_Folder = _config.Value.toolsFolder;
                songeconverter_Folder = _config.Value.toolsFolder;
                ADB_Folder = _config.Value.toolsFolder;
                apktool_Folder = _config.Value.toolsFolder;
                uber_apk_signer_Folder = _config.Value.toolsFolder;
            }



            ProcessStartInfo processtartinfo = new ProcessStartInfo("cmd.exe");
            processtartinfo.Arguments = $"/c erase /s /Q {apkFolder}\\base";

            Process p = new Process();
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c \"{ADB_Folder}\\adb.exe\" pull /data/app/com.beatgames.beatsaber-1/base.apk";
            p = Process.Start(processtartinfo);
            p.WaitForExit();


            processtartinfo.Arguments = $"/c echo n | copy /-y base.apk {backupFolder}\\";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c java -jar \"{apktool_Folder}\\apktool_2.4.0.jar\" d .\\base.apk -o {apkFolder}\\base -f";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c echo n | copy /-y \"{apkFolder}\\base\\lib\\armeabi-v7a\\libil2cpp.so\" {backupFolder}\\";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c \"{songeconverter_Folder}\\songe-converter.exe\" -a {SongsToLoadFolder}";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c \"{BeatMapAssetMaker_Folder}\\BeatMapAssetMaker.exe\" --patch .\\apk\\base\\lib\\armeabi-v7a\\libil2cpp.so";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/K \"{BeatMapAssetMaker_Folder}\\BeatMapAssetMaker.exe\" {apkFolder}\\base\\assets\\bin\\Data\\ .\\assets\\ {SongsToLoadFolder}\\ covers";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c erase /Q \"{apkFolder}\\base\\assets\\bin\\Data\\sharedassets17.assets\"";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c erase /Q \"{apkFolder}\\base\\assets\\bin\\Data\\sharedassets17.assets.split*\"";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c erase /Q \"{apkFolder}\\base\\assets\\bin\\Data\\sharedassets19.assets\"";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/c erase /Q \"{apkFolder}\\base\\assets\\bin\\Data\\sharedassets19.assets.split*\"";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/K copy .\\assets\\*.* \"{apkFolder}\\base\\assets\\bin\\Data\\\"";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/K java -jar \"{apktool_Folder}\\apktool_2.4.0.jar\" b {apkFolder}\\base";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/K \"{JARSIGNERLOC_Folder}\\jarsigner.exe\" -storepass emulamer -keypass emulamer -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore bskey .\\apk\\base\\dist\\base.apk bs";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/K \"{ADB_Folder}\\adb.exe\" uninstall com.beatgames.beatsaber";
            p = Process.Start(processtartinfo);
            p.WaitForExit();

            processtartinfo.Arguments = $"/K \"{ADB_Folder}\\adb.exe\" install {apkFolder}\\base\\dist\\base.apk";
            p = Process.Start(processtartinfo);
            p.WaitForExit();
        }
    }
}
