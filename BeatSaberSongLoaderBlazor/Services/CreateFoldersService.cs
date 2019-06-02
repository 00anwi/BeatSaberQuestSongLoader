using BeatSaberSongLoaderBlazor.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Services
{
    public class CreateFoldersService
    {
        private readonly IOptions<FolderSettings> _config;

        public CreateFoldersService(IOptions<FolderSettings> config)
        {
            _config = config;
        }


    }
}
