﻿using System;
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
using GuessCheck;
using Fitbit.Api.Portable.Models;

namespace Adaptive_Alarm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {

        AppData appData;
        string saveFilename;
        bool afterBootup = false;
        INotificationManager notificationManager;
        int notificationNumber = 0;

        //public string wakeUpTime { get; } = "Waking you up at";

        public  MainPage()
        {
            InitializeComponent();
            saveFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppData.json");
            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };

            if (File.Exists(saveFilename))
            {
                string jsonstring = File.ReadAllText(saveFilename);
                appData = JsonConvert.DeserializeObject<AppData>(jsonstring);
            }
            else
            {
                appData = new AppData();
            }
            //Set time pickers to saved data
            TPMonday.Time = appData.monday;
            TPTuesday.Time = appData.tuesday;
            TPWednesday.Time = appData.wednesday;
            TPThursday.Time = appData.thursday;
            TPFriday.Time = appData.friday;
            TPSaturday.Time = appData.saturday;
            TPSunday.Time = appData.sunday;

            //Set switches to saved data
            tomorrowSwitch.IsToggled = appData.TomorrowSwitchState;
            mondaySwitch.IsToggled = appData.MondaySwitchState;
            tuesdaySwitch.IsToggled = appData.TuesdaySwitchState;
            wednesdaySwitch.IsToggled = appData.WednesdaySwitchState;
            thursdaySwitch.IsToggled = appData.ThursdaySwitchState;
            fridaySwitch.IsToggled = appData.FridaySwitchState;
            saturdaySwitch.IsToggled = appData.SaturdaySwitchState;
            sundaySwitch.IsToggled = appData.SundaySwitchState;

            if ((DateTime.Now - appData.nextChanged).TotalHours > 16){

                appData.next = appData.currTimeSpan();

            }

            TPNext.Time = appData.next;

            if((DateTime.Now - appData.scoreAdded).TotalHours > 16)
            {
                ScorePrompt();
            }

            
             
            /*
            TPMonday.PropertyChanged += "OnTimePickerPropertyChanged";
            TPTuesday.Time = appData.tuesday;
            TPWednesday.Time = appData.wednesday;
            TPThursday.Time = appData.thursday;
            TPFriday.Time = appData.friday;
            TPSaturday.Time = appData.saturday;
            TPSunday.Time = appData.sunday;*/

        }

        void ShowNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var msg = new Label()
                {
                    Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
                };
                stackLayout.Children.Add(msg);
            });
        }

        private async void ScorePrompt()
        {
            HashSet<string> acceptableScores = new HashSet<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"};
            string result = await DisplayPromptAsync("Wakefulness", "How rested did you feel waking up this morning?", placeholder:"Scale 1-10 where 10 is best", maxLength:2, keyboard:Keyboard.Numeric);
          
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                while (!acceptableScores.Contains(result))
                {
                    result = await DisplayPromptAsync("Wakefulness", "Please input a number 1-10", placeholder: "Scale 1-10 where 10 is best", maxLength: 2, keyboard: Keyboard.Numeric);
                    if (!string.IsNullOrWhiteSpace(result)){
                        result = result.Trim(); 
                    }
                }
                int score = Convert.ToInt32(result);
                GaC.addScore(score);
                appData.scoreAdded = DateTime.Now;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);
            }

            afterBootup = true;
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
            int totalMin = GaC.findAlarmTime(appData.currDateTime(), appData.AwakeTime);
            DateTime nTime = DateTime.Now;
            TimeSpan time = TimeSpan.FromMinutes(totalMin);
            DateTime wakeTime = nTime + time;
            //notificationManager.SendNotification("succes?", string.Format("{0:hh:mm tt}", wakeTime));
            //notificationManager.SendNotification("test", "1 min later", DateTime.Now.AddMinutes(1));
            appData.wakeAlarmID = notificationManager.SendNotification("WAKE UP", "IT IS TIME TO WAKE UP", wakeTime.AddMinutes(-1));
            string message = "Initial Alarm set for " + string.Format("{0:hh:mm tt}", wakeTime) 
                + " To wake up before " + appData.currDateTime().ToString();
            //TimeMessage.Text = message;
            await DisplayAlert("Reminder", message, "OK");
            string sonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, sonstring);
        }

        //async void OnScorePressed(object sender, EventArgs e)
        //{
            //await Navigation.PushAsync(new ScorePage());
        //}

        void OnTimePickerPropertyChangedM(object sender, PropertyChangedEventArgs args)
        {
            // Saves all the times to files
            if (args.PropertyName == "Time")
            {
                appData.monday = TPMonday.Time;
                string jsonstring = JsonConvert.SerializeObject(appData);
                File.WriteAllText(saveFilename, jsonstring);

                if (afterBootup)
                    mondaySwitch.IsToggled = true;
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

                if (afterBootup)
                    tuesdaySwitch.IsToggled = true;
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

                if (afterBootup)
                    wednesdaySwitch.IsToggled = true;
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

                if (afterBootup)
                    thursdaySwitch.IsToggled = true;
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

                if (afterBootup)
                    fridaySwitch.IsToggled = true;
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

                if (afterBootup)
                    saturdaySwitch.IsToggled = true;
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

                if (afterBootup)
                    sundaySwitch.IsToggled = true;
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

                if (afterBootup)
                    tomorrowSwitch.IsToggled = true;
            }
        }

        void TomorrowOnToggled(object sender, ToggledEventArgs e)
        {
            appData.TomorrowSwitchState = tomorrowSwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
            //DisplayAlert("Alert", "Tomorrow", "OK");
        }

        void MondayOnToggled(object sender, ToggledEventArgs e)
        {
            appData.MondaySwitchState = mondaySwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        void TuesdayOnToggled(object sender, ToggledEventArgs e)
        {
            appData.TuesdaySwitchState = tuesdaySwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        void WednesdayOnToggled(object sender, ToggledEventArgs e)
        {
            appData.WednesdaySwitchState = wednesdaySwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        void ThursdayOnToggled(object sender, ToggledEventArgs e)
        {
            appData.ThursdaySwitchState = thursdaySwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        void FridayOnToggled(object sender, ToggledEventArgs e)
        {
            appData.FridaySwitchState = fridaySwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        void SaturdayOnToggled(object sender, ToggledEventArgs e)
        {
            appData.SaturdaySwitchState = saturdaySwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        void SundayOnToggled(object sender, ToggledEventArgs e)
        {
            appData.SundaySwitchState = sundaySwitch.IsToggled;
            string jsonstring = JsonConvert.SerializeObject(appData);
            File.WriteAllText(saveFilename, jsonstring);
        }

        // A helper method that checks which checkboxes are checked and updates the alarms accordingly
        void SetAlarms()
        {

            if(tomorrowCheckBox.IsChecked)
                TPNext.Time = TPMultiple.Time;

            if(mondayCheckBox.IsChecked)
                TPMonday.Time = TPMultiple.Time;

            if (tuesdayCheckBox.IsChecked)
                TPTuesday.Time = TPMultiple.Time;

            if (wednesdayCheckBox.IsChecked)
                TPWednesday.Time = TPMultiple.Time;

            if (thursdayCheckBox.IsChecked)
                TPThursday.Time = TPMultiple.Time;

            if (fridayCheckBox.IsChecked)
                TPFriday.Time = TPMultiple.Time;

            if (saturdayCheckBox.IsChecked)
                TPSaturday.Time = TPMultiple.Time;

            if (sundayCheckBox.IsChecked)
                TPSunday.Time = TPMultiple.Time;

        }

        // Helper method that hides checkboxes after user sets or cancels
        void hideCheckboxes()
        {
            tomorrowCheckBox.IsVisible = false;
            mondayCheckBox.IsVisible = false;
            tuesdayCheckBox.IsVisible = false;
            wednesdayCheckBox.IsVisible = false;
            thursdayCheckBox.IsVisible = false;
            fridayCheckBox.IsVisible = false;
            saturdayCheckBox.IsVisible = false;
            sundayCheckBox.IsVisible = false;
        }

        //A helper method that unchecks all checkboxes
        void clearCheckboxes()
        {
            tomorrowCheckBox.IsChecked = false;
            mondayCheckBox.IsChecked = false;
            tuesdayCheckBox.IsChecked = false;
            wednesdayCheckBox.IsChecked = false;
            thursdayCheckBox.IsChecked = false;
            fridayCheckBox.IsChecked = false;
            saturdayCheckBox.IsChecked = false;
            sundayCheckBox.IsChecked = false;

        }

        //Change UI to allow user to set multiple alarms to the same time
        void OnMultiplePressed(object sender, EventArgs e)
        {

            //Make checkboxes appear
            tomorrowCheckBox.IsVisible = true;
            mondayCheckBox.IsVisible = true;
            tuesdayCheckBox.IsVisible = true;
            wednesdayCheckBox.IsVisible = true;
            thursdayCheckBox.IsVisible = true;
            fridayCheckBox.IsVisible = true;
            saturdayCheckBox.IsVisible = true;
            sundayCheckBox.IsVisible = true;

            //Change buttons at the bottom
            sleepNowButton.IsVisible = false;
            setMultipleAlarmsButton.IsVisible = false;
            TPMultiple.IsVisible = true;
            confirmMultipleAlarmsButton.IsVisible = true;
            cancelMultipleAlarmsButton.IsVisible = true;

        }

        //Set alarms based on which checkboxes users clicked
        void onConfirmedPressed(object sender, EventArgs e)
        {
          
            //Make checkboxes invisible
            hideCheckboxes();

            //Call method to check the checkboxes and update alarms
            SetAlarms();

            //Clear checkboxes so that none are checked next time user clicks the button
            clearCheckboxes();

            //Return buttons to normal
            confirmMultipleAlarmsButton.IsVisible = false;
            cancelMultipleAlarmsButton.IsVisible = false;
            TPMultiple.IsVisible = false;
            sleepNowButton.IsVisible = true;
            setMultipleAlarmsButton.IsVisible = true;

        }

        //If user decides not to set alarms allow them to cancel
        void onCancelPressed(object sender, EventArgs e)
        {
            //Hide and clear checkboxes without setting any alarms
            hideCheckboxes();
            clearCheckboxes();

            confirmMultipleAlarmsButton.IsVisible = false;
            cancelMultipleAlarmsButton.IsVisible = false;
            TPMultiple.IsVisible = false;
            sleepNowButton.IsVisible = true;
            setMultipleAlarmsButton.IsVisible = true;

        }
    }
}                       