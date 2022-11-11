using Xamarin.Forms;
using System;
using System.Collections.Generic;
using GaCData;
using Newtonsoft.Json;
using System.IO;
using DataMonitorLib;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;

namespace Adaptive_Alarm
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            // Reads in settings
            var path = Path.Combine(FileSystem.AppDataDirectory, "Settings.json");
            Settings s;
            if (File.Exists(path))
            {
                string settingsJson;
                using (StreamReader streamReader = File.OpenText(path))
                    settingsJson = streamReader.ReadToEndAsync().Result;
                s = JsonConvert.DeserializeObject<Settings>(settingsJson);
            }
            else
            {
                s = new Settings();
            }
            this.Properties["settings"] = s;
            DataMonitor dm;
            if (s.CurrentDeviceType == "None") { dm = new GaCDataMonitor(); }
            else { dm = new FitbitDataMonitor(); }

            dm.LoadState();
            this.Properties["dataMonitor"] = dm;

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            Settings settings = (Settings)this.Properties["settings"];

            var path = Path.Combine(FileSystem.AppDataDirectory, "Settings.json");
            using (StreamWriter streamWriter = File.CreateText(path))
                streamWriter.WriteAsync(JsonConvert.SerializeObject(settings));
        }

        protected override void OnResume()
        {
        }

    }
}
