using BeatSaberSongLoaderBlazor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSaberSongLoaderBlazor.Data
{
    public class QuestStatusService
    {
        private Timer _timer;
        private readonly IOptions<FolderSettings> _folderSettings;
        private StartServerResult serverResult;
        public QuestDeviceInfo questDeviceInfo;

        public QuestStatusService(IOptions<FolderSettings> folderSettings = null)
        {
            _folderSettings = folderSettings;
        }

        public async Task ExecuteAsync()
        {
            questDeviceInfo = new QuestDeviceInfo
            {
                Model = "",
                Serial = "",
                Name = "",
                Product = "",
                State = ""
            };

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
        }

        private void DoWork(object state)
        {
            try
            {
                AdbServer server = new AdbServer();
                if(!AdbServer.Instance.GetStatus().IsRunning)
                {
                    serverResult = server.StartServer(Path.Combine(_folderSettings.Value.ADB_Folder, "platform-tools"), restartServerIfNewer: false);
                }

                var ADBDevices = AdbClient.Instance.GetDevices();

                if(ADBDevices.Any())
                {
                    if(ADBDevices.Count == 1)
                    {
                        if(ADBDevices.First().Model == "Quest")
                        {
                            questDeviceInfo = new QuestDeviceInfo
                            {
                                Model = ADBDevices.First().Model,
                                Serial = ADBDevices.First().Serial,
                                Name = ADBDevices.First().Name,
                                Product = ADBDevices.First().Product,
                                State = ADBDevices.First().State.ToString()
                            };

                            QuestStatusEvent?.Invoke(this, new QuestStatusEventArgs(true, $"{ADBDevices.First().Model} is Connected"));
                        }
                        else
                        {
                            QuestStatusEvent?.Invoke(this, new QuestStatusEventArgs(false, "Quest is Disconnected"));
                        }
                    }
                    else
                    {
                        int foreachloop = 0;
                        bool QuestFound = false;
                        foreach(var ADBDevice in ADBDevices)
                        {
                            if(ADBDevice.Model == "Quest")
                            {
                                questDeviceInfo = new QuestDeviceInfo
                                {
                                    Model = ADBDevice.Model,
                                    Serial = ADBDevice.Serial,
                                    Name = ADBDevice.Name,
                                    Product = ADBDevice.Product,
                                    State = ADBDevice.State.ToString()
                                };

                                QuestStatusEvent?.Invoke(this, new QuestStatusEventArgs(true, $"{ADBDevice.Model} is Connected"));
                                break;
                            }

                            if(foreachloop == ADBDevices.Count())
                            {
                                QuestStatusEvent?.Invoke(this, new QuestStatusEventArgs(false, "Quest is Disconnected"));
                            }

                            foreachloop += 1;
                        }
                    }
                }
                else
                {
                    QuestStatusEvent?.Invoke(this, new QuestStatusEventArgs(false, "Quest is Disconnected"));
                }
                
            }
            catch (Exception e)
            {

            }
        }

        public class QuestDeviceInfo
        {
            public string Model { get; set; }
            public string Serial { get; set; }
            public string Product { get; set; }
            public string State { get; set; }
            public string Name { get; set; }
        }

        public delegate void QuestStatusEventHandler(object sender, QuestStatusEventArgs e);

        public event QuestStatusEventHandler QuestStatusEvent;

        public class QuestStatusEventArgs : EventArgs
        {
            public QuestStatusEventArgs(bool connected, string status)
            {
                Connected = connected;
                Status = status;
            }

            public bool Connected { get; set; }
            public string Status { get; set; }
        }

        public void Stop()    
        {
            _timer?.Change(Timeout.Infinite, 0);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
