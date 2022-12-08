using System;
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

            string saveFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppData.json");
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
            DataMonitor dataMonitor = (DataMonitor)Application.Current.Properties["dataMonitor"];
            //DisplayAlert("Got DataMonitor", "Body", "OK");

            //TODO: change this to be conditional on the currently selected device type.
            ((FitbitDataMonitor)dataMonitor).Authenticate();

            //FOR APPLE
            if (Device.RuntimePlatform == Device.iOS)
            {
                dataMonitor = DependencyService.Get<AppleWatchDataMonitorInterface>();
            }
            else
            {
                Console.WriteLine("Apple watch data collection is only supported on apple devices");
            }
        }

        private void saveButtonClicked(object sender, EventArgs e)
        {
            sleepTime = Convert.ToInt32(SleepTimeNumber.Text);
            string saveFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppData.json");
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