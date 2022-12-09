using System;
using Xamarin.Forms;

namespace Adaptive_Alarm.Views
{

    public partial class TimeScorePage : ContentPage
    {
        public TimeScorePage()
        {
            DevExpress.XamarinForms.Charts.Initializer.Init();
            InitializeComponent();            
        }
        private async void NavigateButton_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScorePage());
        }
    }

    
}