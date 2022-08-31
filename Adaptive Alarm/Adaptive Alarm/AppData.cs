using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System;

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
             * Returns a TimeSpan corresponding to the one that should be used if user slept right now
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
             * Returns a DateTime corresponding to the one that should be used if user slept right now
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

    }
}
