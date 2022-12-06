using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Shiny;
using Shiny.Jobs;
using Shiny.Notifications;

using DataMonitorLib;

namespace Adaptive_Alarm
{
    public class BackgroundJob : IJob
    {
        readonly INotificationManager notificationManager;
        public BackgroundJob(INotificationManager notificationManager) => this.notificationManager = notificationManager;


        public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            DataMonitor dm = (DataMonitor)App.Current.Properties["dataMonitor"];
            //await Task.Run(() => Console.WriteLine($"BACKGROUNDJOB - {dm.GetType().Name}")); //for debugging purposes

            //TODO: Uncomment these to allow data collection
            await Task.Run(() => dm.CollectDataPoint(), cancelToken);
            await Task.Run(() => dm.SaveState(), cancelToken);

            // TODO: add code to update alarm time and UI element for tomorrow's alarm time according to the DataMonitor's new state.
        }
    }
}
