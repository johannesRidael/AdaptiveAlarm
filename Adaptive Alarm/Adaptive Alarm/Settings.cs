using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Adaptive_Alarm
{
    /// <summary>
    /// Outlines a class to hold information on the current application settings.
    /// </summary>
    [KnownType(typeof(Settings))]
    public class Settings
    {
        public string CurrentDeviceType;
        public DateTime CurrentDataSetStartDate;

        public Settings()
        {
            this.CurrentDeviceType = "None";
            this.CurrentDataSetStartDate = DateTime.Now;
        }

        public Settings(string currentDeviceType, DateTime currentDataSetStartDate)
        {
            this.CurrentDeviceType = currentDeviceType;
            this.CurrentDataSetStartDate = currentDataSetStartDate;
        }
    }
}
