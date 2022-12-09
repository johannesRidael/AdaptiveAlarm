using Adaptive_Alarm.Data;
using System.Collections.Generic;
using System;

namespace Adaptive_Alarm.ViewModels {
    public class PulseViewModel {
        public List<PulseItem> Data { get; }
        public PulseViewModel()
        {
            //var dt1 = DateTime.Now;
            //var hm1 = new DateTime(dt1.Hour, dt1.Minute, 0);
            //var dt2 = dt1.AddHours(1.0);
            //var hm2 = new DateTime(dt2.Hour, dt2.Minute, 0);
            //var dt3 = dt2.AddHours(1.0);
            //var hm3 = new DateTime(dt3.Hour, dt3.Minute, 0);
            //var dt4 = dt3.AddHours(1.0);
            //var hm4 = new DateTime(dt4.Hour, dt4.Minute, 0);
            //var dt5 = dt4.AddHours(1.0);
            //var hm5 = new DateTime(dt5.Hour, dt5.Minute, 0);
            //var dt6 = dt5.AddHours(1.0);
            //var hm6 = new DateTime(dt6.Hour, dt6.Minute, 0);

            Data = new List<PulseItem>() {               
                new PulseItem() { Argument = 0, Value = 25},
                new PulseItem() { Argument = 1, Value = 45 },
                new PulseItem() { Argument = 2, Value = 50 },
                new PulseItem() { Argument = 3, Value = 46 },
                new PulseItem() { Argument = 4, Value = 42 },
                new PulseItem() { Argument = 5, Value = 47 },
                new PulseItem() { Argument = 6, Value = 50 },
                new PulseItem() { Argument = 7, Value = 55 },
                new PulseItem() { Argument = 8, Value = 70 }

            };
        }
    }
    public class PulseItem
    {
        public double Argument { get; set; }
        public double Value { get; set; }
    }
}
