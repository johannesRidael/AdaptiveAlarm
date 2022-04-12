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
            this.webView.Source = "https://www.google.com";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DisplayAlert("Alert!", "Doing things as the WebView Appears", "Close Alert");
        }
    }
}