using Microsoft.Extensions.DependencyInjection;
using Shiny;
using System;
using System.Collections.Generic;
using System.Text;
using Shiny.Jobs;

namespace Adaptive_Alarm
{
    public class MyShinyStartup : ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services, IPlatform platform)
        {
            // this is where you'll load things like BLE, GPS, etc - those are covered in other sections
            // things like the jobs, environment, power, are all installed automatically
            services.UseJobs(true);
            services.UseNotifications(); // adding notifications for some job fun

            services.RegisterJob(typeof(BackgroundJob));
        }
    }
}