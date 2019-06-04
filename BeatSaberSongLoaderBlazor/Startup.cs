using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BeatSaberSongLoaderBlazor.Data;
using BeatSaberSongLoaderBlazor.Services;
using Microsoft.Extensions.Configuration;
using BeatSaberSongLoaderBlazor.Models;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace BeatSaberSongLoaderBlazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddOptions();

            services.AddSingleton<SongsLoaderService>();
            services.AddSingleton<BeatSaverScraperService>();
            services.AddSingleton<InitializeService>();

            services.AddSingleton<QuestStatusService>();

            services.Configure<FolderSettings>(Configuration.GetSection("FolderSettings"));
            services.ConfigureWritable<FolderSettings>(Configuration.GetSection("FolderSettings"));

            services.Configure<Dependencies>(Configuration.GetSection("Dependencies"));
            services.ConfigureWritable<Dependencies>(Configuration.GetSection("Dependencies"));

            services.Configure<Settings>(Configuration.GetSection("Settings"));
            services.ConfigureWritable<Settings>(Configuration.GetSection("Settings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });

            CreateFolders();
        }

        public void CreateFolders()
        {
            try
            {
                var SongsToLoadfolder = Configuration.GetSection("FolderSettings")["SongsToLoadFolder"];
                // If the directory doesn't exist, create it.
                if (!Directory.Exists(SongsToLoadfolder))
                {
                    Directory.CreateDirectory(SongsToLoadfolder);
                }

                var apkFolder = Configuration.GetSection("FolderSettings")["apkFolder"];
                if (!Directory.Exists(apkFolder))
                {
                    Directory.CreateDirectory(apkFolder);
                }

                var assetsFolder = Configuration.GetSection("FolderSettings")["assetsFolder"];
                if (!Directory.Exists(assetsFolder))
                {
                    Directory.CreateDirectory(assetsFolder);
                }

                var backupFolder = Configuration.GetSection("FolderSettings")["backupFolder"];
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                var toolsFolder = Configuration.GetSection("FolderSettings")["toolsfolder"];
                if (!Directory.Exists(toolsFolder))
                {
                    Directory.CreateDirectory(toolsFolder);
                }
            }
            catch (Exception)
            {
                // Fail silently
            }
        }

    }
}
