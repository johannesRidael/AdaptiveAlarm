using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;
using Xamarin.Essentials;

namespace Utility
{
    public class AppData
    {
        public TimeSpan monday { get; set; }
        public TimeSpan tuesday { get; set; }
        public TimeSpan wednesday { get; set; }
        public TimeSpan thursday { get; set; }
        public TimeSpan friday { get; set; }
        public TimeSpan saturday { get; set; }
        public TimeSpan sunday { get; set; }
        public TimeSpan next { get; set; }
        //private TimeSpan[] listVer;
        public DateTime nextChanged { get; set; }
        public DateTime scoreAdded { get; set; }
        public int AwakeTime { get; set; }


        public bool TomorrowSwitchState { get; set; }
        public bool MondaySwitchState { get; set; }
        public bool TuesdaySwitchState { get; set; }
        public bool WednesdaySwitchState { get; set; }
        public bool ThursdaySwitchState { get; set; }
        public bool FridaySwitchState { get; set; }
        public bool SaturdaySwitchState { get; set; }
        public bool SundaySwitchState { get; set; }
        public int wakeAlarmID { get; set; }



        public AppData()
        {
            monday = new TimeSpan(7, 0, 0);
            tuesday = new TimeSpan(7, 0, 0);
            wednesday = new TimeSpan(7, 0, 0);
            thursday = new TimeSpan(7, 0, 0);
            friday = new TimeSpan(7, 0, 0);
            saturday = new TimeSpan(7, 0, 0);
            sunday = new TimeSpan(7, 0, 0);
            next = new TimeSpan(7, 0, 0);
            AwakeTime = 0;
            scoreAdded = DateTime.Now.AddDays(-3);
            nextChanged = DateTime.Now.AddDays(-3);
            wakeAlarmID = -1;
            // = new TimeSpan[] { next, monday, tuesday, wednesday, thursday, friday, saturday, sunday }
        }

        public TimeSpan GetTimeSpan(DayOfWeek Day)
        {
            /*
             * Returns the timespan corresponding to the DayOfWeek given
             */
            switch (Day)
            {
                case DayOfWeek.Sunday:
                    return sunday;
                case DayOfWeek.Monday:
                    return monday;
                case DayOfWeek.Tuesday:
                    return tuesday;
                case DayOfWeek.Wednesday:
                    return wednesday;
                case DayOfWeek.Thursday:
                    return thursday;
                case DayOfWeek.Friday:
                    return friday;
                case DayOfWeek.Saturday:
                    return saturday;
                default:
                    return TimeSpan.Zero;
            }
        }

        public TimeSpan currTimeSpan()
        {
            /*
             * Returns a TimeSpan corresponding to the one that should be used if user slept right now (the next alarm time)
             */
            if ((DateTime.Now - nextChanged).TotalHours < 16)
            {
                return next;
            }
            DateTime now = DateTime.Now;
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            TimeSpan todays = GetTimeSpan(now.DayOfWeek);
            TimeSpan tomos = GetTimeSpan(tomorrow.DayOfWeek);
            today = today + todays;
            tomorrow = tomorrow + tomos;


            if ((today - now).TotalMinutes < 0)
            {
                return tomos;
            }
            return todays;
        }

        public DateTime currDateTime()
        {
            /*
             * Returns a DateTime corresponding to the one that should be used if user slept right now (the next alarm time)
             */
            DateTime now = DateTime.Now;
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            TimeSpan todays = GetTimeSpan(now.DayOfWeek);
            TimeSpan tomos = GetTimeSpan(tomorrow.DayOfWeek);
            if ((DateTime.Now - nextChanged).TotalHours < 16)
            {
                todays = next;
                tomos = next;
            }
            today = today + todays;
            tomorrow = tomorrow + tomos;


            if ((today - now).TotalMinutes < 0)
            {
                return tomorrow;
            }
            return today;

        }

        /// <summary>
        /// Loads an instance from persistent storage
        /// </summary>
        /// <returns></returns>
        public static AppData Load()
        {
            string saveFilename = Path.Combine(FileSystem.AppDataDirectory, "AppData.json");
            AppData appData;
            if (File.Exists(saveFilename))
            {
                string jsonstring = File.ReadAllText(saveFilename);
                appData = JsonConvert.DeserializeObject<AppData>(jsonstring);
            }
            else
            {
                appData = new AppData();
            }
            return appData;
        }

        /// <summary>
        /// Saves the current instance to disk
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Save()
        {
            if (this != null)
            {
                string saveFilename = Path.Combine(FileSystem.AppDataDirectory, "AppData.json");
                string sonstring = JsonConvert.SerializeObject(this);
                File.WriteAllText(saveFilename, sonstring);
            }
            else
            {
                throw new ArgumentNullException(nameof(AppData));
            }
        }
    }
}
