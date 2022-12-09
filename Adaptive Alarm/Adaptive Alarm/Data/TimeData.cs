using System.Collections.Generic;
using DevExpress.XamarinForms.Charts;
using DataMonitorLib;

namespace Adaptive_Alarm.Data {
    public class TimeData : IPieSeriesData {
        List<KeyValuePair<string, double>> data;

        public TimeData() {
            //data = DataMonitorLib.FitbitDataMonitor.GetSleepSessionCollectionTimes();

            // Display test only
            data = new List<KeyValuePair<string, double>>() {
                new KeyValuePair<string, double>("REM", 3.65),
                new KeyValuePair<string, double>("Deep", 1.55),
                new KeyValuePair<string, double>("Light", 2.45),
                new KeyValuePair<string, double>("Wake", 0.45)
            };
        }
        public int GetDataCount() => data.Count;
        public string GetLabel(int index) => data[index].Key;
        public double GetValue(int index) => data[index].Value;
        public object GetKey(int index) => null;
    }
}
