using Xamarin.Forms;
using System;
using System.Collections.Generic;
using GaCData;
using Newtonsoft.Json;
using System.IO;
using DataMonitorLib;

namespace Adaptive_Alarm
{
    public partial class App : Application
    {
        //TODO: wrap this and any other needed variables into an object that will be stored and loaded into local memory on app close/open.
        private string deviceType = "Fitbit"; //TODO: change default to None once we have a DataMonitor to match that.
        private DataMonitor dataMonitor = new FitbitDataMonitor(); //TODO: make this build based upon application settings and set the default to "NoneDataMonitor"

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            this.Properties["dataMonitor"] = dataMonitor;
        }

        protected override void OnStart()
        {
            //TODO: read in settings and set GUI element values appropriately

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

    }
}
