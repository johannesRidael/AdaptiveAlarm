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
            // Specify that we want to use Shiny Background Jobs.
            services.UseJobs(true);

            services.RegisterJob(typeof(BackgroundJob));
        }
    }
}