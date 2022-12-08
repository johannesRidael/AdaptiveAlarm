using Xamarin.Forms;
using System;
using System.Collections.Generic;
using GaCData;
using Newtonsoft.Json;
using System.IO;
using DataMonitorLib;
using Adaptive_Alarm;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;

namespace Adaptive_Alarm
{
    public partial class App : Application
    {
        public DataMonitor dm { get; set; }
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            // Provides default settings if it cannot find a prior value
            if (!this.Properties.ContainsKey("CurrentDeviceType"))
                this.Properties["CurrentDeviceType"] = "None";
            if (!this.Properties.ContainsKey("CurrentDataSetStartDate"))
                this.Properties["CurrentDataSetStartDate"] = DateTime.Now;

            DataMonitor dm;
            if (this.Properties["CurrentDeviceType"].Equals("None")) { dm = new GaCDataMonitor(); }
            else { dm = new FitbitDataMonitor(); }

            dm.LoadState();
            this.Properties["dataMonitor"] = dm;

            Application.Current.Properties["isInForeground"] = false;
        }

        protected override void OnStart()
        {
            Application.Current.Properties["isInForeground"] = true;
        }

        protected override void OnSleep()
        {
            Application.Current.Properties["isInForeground"] = false;
        }

        protected override void OnResume()
        {
            Application.Current.Properties["isInForeground"] = true;
        }

    }
}
