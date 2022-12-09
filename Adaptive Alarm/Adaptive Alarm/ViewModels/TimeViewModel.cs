using Adaptive_Alarm.Data;

namespace Adaptive_Alarm.ViewModels {
    public class TimeViewModel {
        readonly TimeData donutSeriesData;

        public TimeData DonutSeriesData => donutSeriesData;
        public string DonutCenterLabelPattern => "Total\n{TV}h";

        public TimeViewModel() {
            donutSeriesData = new TimeData();
        }
    }
}
