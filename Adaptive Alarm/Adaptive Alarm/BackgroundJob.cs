﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Shiny;
using Shiny.Jobs;

using DataMonitorLib;
using Xamarin.Forms;
using Utility;

namespace Adaptive_Alarm
{
    public class BackgroundJob : IJob
    {
        public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            DataMonitor dm = (DataMonitor)App.Current.Properties["dataMonitor"];
            //await Task.Run(() => Console.WriteLine($"BACKGROUNDJOB - {dm.GetType().Name}")); //for debugging purposes

            // These allow data collection
            await Task.Run(() => dm.CollectDataPoint(), cancelToken);
            await Task.Run(() => dm.SaveState(), cancelToken);

            // TODO: add code to update alarm time and UI element for tomorrow's alarm time according to the DataMonitor's new state.
            INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
            int alarmId = AppData.Load().wakeAlarmID;
            DateTime wakeTime = dm.EstimateWakeupTime();
            if (alarmId == 0) { notificationManager.SendNotification("WAKE UP", "IT IS TIME TO WAKE UP", wakeTime); }
            else { notificationManager.updateNotification("WAKE UP", "IT IS TIME TO WAKE UP", wakeTime, alarmId); }

        }
    }
}
