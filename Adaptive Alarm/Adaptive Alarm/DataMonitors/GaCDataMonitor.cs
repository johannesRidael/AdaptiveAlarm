using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fitbit.Api.Portable;
using Fitbit.Models;
using Newtonsoft.Json;

namespace DataMonitorLib
{
    class GaCDataMonitor : DataMonitor
    {
        public override void ClearState()
        {
            throw new NotImplementedException();
        }

        public override void CollectDataPoint()
        {
            throw new NotImplementedException();
        }

        public override DateTime EstimateWakeupTime()
        {
            throw new NotImplementedException();
        }

        public override void LoadState()
        {
            //TODO: If a prior state was saved locally then load it in.
            throw new NotImplementedException();
        }

        public override void SaveState()
        {
            //TODO: Save the current state locally.
            throw new NotImplementedException();
        }


    }
}
