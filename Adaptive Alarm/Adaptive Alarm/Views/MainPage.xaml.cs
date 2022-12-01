using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using DataMonitorLib;
using Xamarin.Essentials;
using Shiny.Jobs;
using Shiny;

namespace Adaptive_Alarm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {

        AppData appData;
        string saveFilename;

        public  MainPage()
        {
            InitializeComponent();
            saveFilename = Path.Combine(FileSystem.AppDataDirectory, "AppData.json");

            if (File.Exists(saveFilename))
            {
                string jsonstring = File.ReadAllText(saveFilename);
                appData = JsonConvert.DeserializeObject<AppData>(jsonstring);
            }
            else
            {
                appData = new AppData();
            }
            TPMonday.Time = appData.monday;
            TPTuesday.Time = appData.tuesday;
            TPWednesday.Time = appData.wednesday;
            TPThursday.Time = appData.thursday;
            TPFriday.Time = appData.friday;
            TPSaturday.Time = appData.saturday;
            TPSunday.Time = appData.sunday;

            
            

            if((DateTime.Now - appData.nextChanged).TotalHours > 16){

                appData.next = appData.currTimeSpan();

            }

            TPNext.Time = appData.next;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current.Properties["CurrentDeviceType"].Equals("None") && (bool)Application.Current.Properties["isInForeground"] && (bool)Application.Current.Properties["gacNeedsNewData"])
            {
                GaCDataMonitor dm = (GaCDataMonitor)App.Current.Properties["dataMonitor"];
                dm.PromptForData();
            }
        }

        async void OnSleepPressed(object sender, EventArgs e)
        {
            if (File.Exists(saveFilename))
            {
                string jsonstring = File.ReadAllText(saveFilename);
                appData = JsonConvert.DeserializeObject<AppData>(jsonstring);
            }
            else
            {
                appData = new AppData();
            }
            DataMonitor dm = (DataMonitor)App.Current.Properties["dataMonitor"];
            DateTime wakeTime = dm.EstimateWakeupTime(); //TODO: use this to update the alarm notification automatically
            string message = "Please set your alarm for " + string.Format("{0:hh:mm tt}", wakeTime) 
                + " To wake up before " + appData.currDateTime().ToString();
            //TimeMessage.Text = message;
            await DisplayAlert("Reminder", message, "OK");
        }

        async void OnScorePressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScorePage());
        }

        #region Timepicker listeners
        void OnTimePickerPropertyChangedM(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.monday = TPMonday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }
        }

        void OnTimePickerPropertyChangedTu(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.tuesday = TPTuesday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }
        }

        void OnTimePickerPropertyChangedW(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.wednesday = TPWednesday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }
        }

        void OnTimePickerPropertyChangedTh(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.thursday = TPThursday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }
        }

        void OnTimePickerPropertyChangedF(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.friday = TPFriday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }
        }

        void OnTimePickerPropertyChangedSa(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.saturday = TPSaturday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }
        }

        void OnTimePickerPropertyChangedSu(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.sunday = TPSunday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }
        }

        void OnTimePickerPropertyChangedNe(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.next = TPNext.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
                appData.nextChanged = DateTime.Now;
            }
        }
        #endregion
    }
}                       