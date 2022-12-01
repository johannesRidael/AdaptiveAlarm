using DataMonitorLib;
using Foundation;
using HealthKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using HealthKit;

namespace Adaptive_Alarm.iOS
{
    internal class AppleWatchDataMonitor : DataMonitor
    {
        
        public static void RequestHKAccess()
        {
            HKHealthStore healthKitStore = new HKHealthStore();


            var sleepAnalysis = HKCategoryTypeIdentifierKey.SleepAnalysis;
            var sleepAType = HKObjectType.GetQuantityType(sleepAnalysis);
            var typesToWrite = new NSSet();
            var typesToRead = new NSSet(sleepAType);
            healthKitStore.RequestAuthorizationToShare(
                    typesToWrite,
                    typesToRead,
                    ReactToHealthCarePerms);
        }
            
        private static void ReactToHealthCarePerms(bool success, NSError error)
            {
            HKHealthStore healthKitStore = new HKHealthStore();
            var access = healthKitStore.GetAuthorizationStatus(HKObjectType.GetQuantityType(HKQuantityTypeIdentifierKey.HeartRate));
              if (access.HasFlag(HKAuthorizationStatus.SharingAuthorized))
                {
                    //Success
                }
                else
                {
                    //Failed
                }
            }
        public override void CollectDataPoint()
        {
            throw new NotImplementedException();
        }

        public override DateTime EstimateWakeupTime()
        {
            throw new NotImplementedException();
        }

        public override List<DateTime> GetSleepSessionCollectionTimes()
        {
            throw new NotImplementedException();
        }

        public override void LoadState()
        {
            throw new NotImplementedException();
        }

        public override void SaveState()
        {
            throw new NotImplementedException();
        }
    }
}