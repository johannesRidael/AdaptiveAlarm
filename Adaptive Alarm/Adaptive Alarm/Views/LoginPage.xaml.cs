using DataMonitorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Adaptive_Alarm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            //this.webView.Source = "https://www.google.com";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //TODO: make the auth process conditional on the device type in a settings object.
            FitbitDataMonitor dataMonitor = (FitbitDataMonitor)Application.Current.Properties["dataMonitor"];

            this.webView.Source = dataMonitor.GetAuthUrl();
            
            //DisplayAlert("Url", dataMonitor.getAuthUrl(), "close");
            //this.webView.Reload();
        }

        private void webView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("http://localhost:4306/aa/data-collection")) //Fitbit code is being passed back to us
            {
                //TODO: make the auth process conditional on the device type in a settings object.
                FitbitDataMonitor dataMonitor = (FitbitDataMonitor)Application.Current.Properties["dataMonitor"];
                string code = e.Url.Split('=')[1];
                code = code.Substring(0, code.Length - 2);

                DisplayAlert("Auth Code", code, "close");
                dataMonitor.GetToken(code);
                e.Cancel = true;
            }
        }
    }
}