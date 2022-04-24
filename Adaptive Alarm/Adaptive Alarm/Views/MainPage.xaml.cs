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

namespace Adaptive_Alarm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {

        AppData appData;
        string saveFilename;
        public MainPage()
        {
            InitializeComponent();
            saveFilename = "AppData.Json";
            
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
            /*if((DateTime.Now - appData.nextChanged).TotalHours > 16){
             *  
             *  appData.next = appData.currTimeSpan()}
             * 
             * TPNext.Time = appData.next
             */

        }
        void updateAppData()
        {
            appData.monday = TPMonday.Time;
            appData.tuesday = TPTuesday.Time;
            appData.wednesday = TPWednesday.Time;
            appData.thursday = TPThursday.Time;
            appData.friday = TPFriday.Time;
            appData.saturday = TPSaturday.Time;
            appData.sunday = TPSunday.Time;
            /*if(appData.next != TPNext.Time){
             *  appData.nextChanged = DateTime.Now;
             *  appData.next = TPNext.Time;
             */
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        void OnTimePickerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Time")
            {
                updateAppData();
            }
        }
    }
}                       