using BeatSaberSongLoaderBlazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Data.api
{
    [Route("api/[controller]/[action]/")]
    public class DataTables : Controller
    {
        private readonly IOptions<FolderSettings> _config;
        private readonly SongsLoaderService _songsLoader;

        public DataTables(IOptions<FolderSettings> config,
            SongsLoaderService songsLoader)
        {
            _config = config;
            _songsLoader = songsLoader;
        }

        [HttpPost]
        public IActionResult GetAvailableSongs()
        {
            var list = _songsLoader.AvailableSongs.ToList();

            // Custom response to bind information in client side
            dynamic response = new
            {
                Data = list,
                Draw = 1,
                RecordsFiltered = list.Count,
                RecordsTotal = list.Count
            };
            return Ok(response);
        }

        [HttpPost]
        public IActionResult GetSongsToLoad()
        {
            var list = _songsLoader.SongsToLoad.ToList();

            // Custom response to bind information in client side
            dynamic response = new
            {
                Data = list,
                Draw = 1,
                RecordsFiltered = list.Count,
                RecordsTotal = list.Count
            };
            return Ok(response);
        }
    }
}
