﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DataMonitorLib;
using System.IO;
using Utility;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace Adaptive_Alarm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        int sleepTime;

        public List<string> deviceTypes = new List<string>(){ "None", "Fitbit" };
        public SettingsPage()
        {
            
            InitializeComponent();

            string saveFilename = Path.Combine(FileSystem.AppDataDirectory, "AppData.json");
            AppData data;
            string jsonstring;
            if (File.Exists(saveFilename))
            {
                jsonstring = File.ReadAllText(saveFilename);
                data = JsonConvert.DeserializeObject<AppData>(jsonstring);
            }
            else
            {
                data = new AppData();
            }
            SleepTimeNumber.Text = data.AwakeTime.ToString();

        }

        private void ChangeDeviceButtonClicked(object sender, EventArgs e)
        {
            DataMonitor dataMonitor;
            Settings settings = (Settings)Application.Current.Properties["settings"];

            if ((string)typePicker.SelectedItem == "None")
            {
                dataMonitor = new GaCDataMonitor();
            }
            else
            {
                dataMonitor = new FitbitDataMonitor();
                ((FitbitDataMonitor)dataMonitor).Authenticate();
            }
            settings.CurrentDeviceType = (string)typePicker.SelectedItem;
            Application.Current.Properties["dataMonitor"] = dataMonitor;
            Application.Current.Properties["settings"] = settings;
        }

        private void saveButtonClicked(object sender, EventArgs e)
        {
            sleepTime = Convert.ToInt32(SleepTimeNumber.Text);
            string saveFilename = Path.Combine(FileSystem.AppDataDirectory, "AppData.json");
            AppData data;
            string jsonstring;
            if (File.Exists(saveFilename)){
                jsonstring = File.ReadAllText(saveFilename);
                data = JsonConvert.DeserializeObject<AppData>(jsonstring); 
            }
            else
            {
                data = new AppData();
            }
            data.AwakeTime = sleepTime;
            jsonstring = JsonConvert.SerializeObject(data);
            File.WriteAllText(saveFilename, jsonstring);
            
        }
    }
}