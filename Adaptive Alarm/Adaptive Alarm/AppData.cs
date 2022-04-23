using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System;

namespace AppData
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

        public AppData()
        {
            monday = new TimeSpan(7, 0, 0);
            tuesday = new TimeSpan(7, 0, 0);
            wednesday = new TimeSpan(7, 0, 0);
            thursday = new TimeSpan(7, 0, 0);
            friday = new TimeSpan(7, 0, 0);
            saturday = new TimeSpan(7, 0, 0);
            sunday = new TimeSpan(7, 0, 0);
        }
    }
}
