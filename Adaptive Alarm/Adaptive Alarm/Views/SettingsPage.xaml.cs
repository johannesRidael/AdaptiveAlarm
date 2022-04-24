using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DataMonitorLib;

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
        }

        private void ChangeDeviceButtonClicked(object sender, EventArgs e)
        {
            DataMonitor dataMonitor = (DataMonitor)Application.Current.Properties["dataMonitor"];
            //DisplayAlert("Got DataMonitor", "Body", "OK");

            //TODO: change this to be conditional on the currently selected device type.
            ((FitbitDataMonitor)dataMonitor).Authenticate();
        }

        private void saveButtonClicked(object sender, EventArgs e)
        {
            sleepTime = Convert.ToInt32(SleepTimeNumber.Text);
        }
    }
}