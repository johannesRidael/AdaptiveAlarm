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
        //TODO: wrap this and any other needed variables into an object that will be stored and loaded into local memory on app close/open.
        private DataMonitor dataMonitor = new FitbitDataMonitor(); //TODO: make this build based upon application settings and set the default to "NoneDataMonitor"

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            this.Properties["dataMonitor"] = dataMonitor;
        }

        protected override void OnStart()
        {
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
            this.Properties["currentDataSetStart"] = s.CurrentDataSetStartDate;
            DataMonitor dm;
            if (s.CurrentDeviceType == "None") { dm = new GaCDataMonitor(); }
            else { dm = new FitbitDataMonitor(); }
            
            dm.LoadState();
            this.Properties["dataMonitor"] = dm;
            
        }

        protected override void OnSleep()
        {
            // Saves settings
            var dm = this.Properties["dataMonitor"];
            string s;
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
            if (dm.GetType() is GaCDataMonitor)
                s = "None";
            else s = "Fitbit";
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
            Settings settings = new Settings(s, (DateTime)this.Properties["currentDataSetStart"]);

            var path = Path.Combine(FileSystem.AppDataDirectory, "Settings.json");
            using (StreamWriter streamWriter = File.CreateText(path))
                streamWriter.WriteAsync(JsonConvert.SerializeObject(settings));
        }

        protected override void OnResume()
        {
        }

    }
}
