using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Shiny;
using Shiny.Jobs;
using Shiny.Notifications;

namespace Adaptive_Alarm
{
    public class BackgroundJob : IJob
    {
        readonly INotificationManager notificationManager;
        public BackgroundJob(INotificationManager notificationManager) => this.notificationManager = notificationManager;


        public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            await this.notificationManager.Send("Job Started",$"{jobInfo.Identifier} Started");
            var seconds = jobInfo.Parameters.Get("SecondsToRun", 10);
            await Task.Delay(TimeSpan.FromSeconds(seconds), cancelToken);

            await this.notificationManager.Send("Job Finished",$"{jobInfo.Identifier} Finished");
        }
    }
}
