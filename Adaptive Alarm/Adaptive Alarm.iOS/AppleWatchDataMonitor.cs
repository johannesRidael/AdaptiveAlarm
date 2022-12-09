using DataMonitorLib;
using Foundation;
using HealthKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Newtonsoft.Json;
using Utility;
using System.IO;
using Adaptive_Alarm;
[assembly: Xamarin.Forms.Dependency(typeof(AppleWatchDataMonitor))]
namespace DataMonitorLib
{
    public class AppleWatchDataMonitor : AppleWatchDataMonitorInterface
    {

        DateTime lastSample = DateTime.MinValue;
        DateTime lastEndDate = DateTime.MinValue;
        double[] averages = new double[60];
        int[] sampleCounts = new int[60];
        int curLoc = 0;
        double curMinIn = 0;
        


        public void RequestHKAccess()
        {
            HKHealthStore healthKitStore = new HKHealthStore();


            var SType = HKCategoryType.Create(HKCategoryTypeIdentifier.SleepAnalysis);
            var typesToWrite = new NSSet();
            var typesToRead = new NSSet(SType);
            healthKitStore.RequestAuthorizationToShare(
                    typesToWrite,
                    typesToRead,
                    ReactToHealthCarePerms);
        }
            
        private void ReactToHealthCarePerms(bool success, NSError error)
            {
            HKHealthStore healthKitStore = new HKHealthStore();
            var access = healthKitStore.GetAuthorizationStatus(HKCategoryType.Create(HKCategoryTypeIdentifier.SleepAnalysis));
            if (access.HasFlag(HKAuthorizationStatus.SharingAuthorized))
            {
                //Success
                //dumpData();
                HKHealthStore healthStore = new HKHealthStore();
                var sleepType = HKCategoryType.Create(HKCategoryTypeIdentifier.SleepAnalysis);
                var desc = new HKQueryDescriptor[] { new HKQueryDescriptor(sleepType, null) };
                //var q = new HKSampleQuery()
                var query = new HKSampleQuery(desc, 240, new NSSortDescriptor[] { new NSSortDescriptor(HKSample.SortIdentifierEndDate, false) }, initQueryHandler);
                healthStore.ExecuteQuery(query);
            }
            else
            {
                //Failed
            }
            }

        public static void dumpData()
        {
            HKHealthStore healthStore = new HKHealthStore();
            var sleepType = HKCategoryType.Create(HKCategoryTypeIdentifier.SleepAnalysis);
            var desc = new HKQueryDescriptor[] { new HKQueryDescriptor(sleepType, null) };
            var query = new HKSampleQuery(desc, 30, (quer, tmpResult, error) =>
            {
                if (error != null)
                    Console.WriteLine("Error...Aborted");
                else
                {
                    Console.WriteLine("*********DATA DUMP**********************************************************");
                    foreach (var item in tmpResult)
                    {
                        Console.WriteLine(item.ToString());
                    }
                }
            });
            healthStore.ExecuteQuery(query);

        }

        public AppleWatchDataMonitor()
        {
            for (int i = 0; i < sampleCounts.Length; i++)
            {
                sampleCounts[i] = 0;
                averages[i] = 0;
            }
            RequestHKAccess();
        }

        private void initQueryHandler(HKSampleQuery query, HKSample[] results, NSError error)
        {
            if (error != null)
            {
                Console.WriteLine("Error with HK Query");
                return;
            }
            HKCategorySample[] samples = (HKCategorySample[])results;
            bool beginKeeping = false;
            DateTime endOfLastSample = DateTime.MinValue;
            int index = 0;
            for (int i = results.Length - 1; i >= 0; i--)
            {
                if (samples[i].Value == ((int)HKCategoryValueSleepAnalysis.InBed)) { }
                //do not act on InBed samples
                else
                {
                    if (!beginKeeping)
                    {
                        if (endOfLastSample == DateTime.MinValue)
                        {
                            endOfLastSample = (DateTime)samples[i].EndDate;
                        }
                        else
                        {
                            if ((DateTime)samples[i].StartDate != endOfLastSample)
                            {
                                beginKeeping = true;
                            }//we start counting samples when we have a skip in sample time
                        }
                    }
                    if (beginKeeping)
                    {
                        if ((DateTime)samples[i].StartDate != endOfLastSample)
                        {
                            index = 0;
                        }
                        if (flattenSampleToVal(samples[i]) != index % 2)
                        {
                            index++;
                        }
                        averages[index] = (averages[index] * sampleCounts[index] + ((double)((DateTime)samples[i].EndDate - (DateTime)samples[i].StartDate).TotalSeconds / 60))/ sampleCounts[index] + 1;
                        sampleCounts[index]++;
                    }
                }

            }
        }

        private int flattenSampleToVal(HKCategorySample s)
        {
            /*
             * returns 0 if sample s is wakeable, 1 if sample is not
             */
            int[] wakeable = new int[] { (int)HKCategoryValueSleepAnalysis.Awake, (int)HKCategoryValueSleepAnalysis.AsleepCore };

            HashSet<int> wakeableS = new HashSet<int>(wakeable);

            if (wakeableS.Contains((int)s.Value))
                return 0;
            return 1;
        }

        public override void CollectDataPoint()
        {
            HKHealthStore healthStore = new HKHealthStore();
            var sleepType = HKCategoryType.Create(HKCategoryTypeIdentifier.SleepAnalysis);
            var desc = new HKQueryDescriptor[] { new HKQueryDescriptor(sleepType, null) };
            //var q = new HKSampleQuery()
            var query = new HKSampleQuery(desc, 60, new NSSortDescriptor[] { new NSSortDescriptor(HKSample.SortIdentifierEndDate, false) }, collectQueryHandler);
            healthStore.ExecuteQuery(query);
        }

        private void collectQueryHandler(HKSampleQuery query, HKSample[] results, NSError error)
        {
            if (error != null)
            {
                Console.WriteLine("Error with HK Query");
                return;
            }
            lastSample = DateTime.Now;
            HKCategorySample[] samples = (HKCategorySample[])results;
            bool beginKeeping = false;
            DateTime endOfLastSample = DateTime.MinValue;
            for (int i = results.Length - 1; i >= 0; i--)
            {
                if ((DateTime)samples[i].EndDate >= lastSample)
                {
                    if (samples[i].Value == ((int)HKCategoryValueSleepAnalysis.InBed)) { }
                    //do not act on InBed samples
                    else
                    {
                        if (beginKeeping)
                        {
                            if ((DateTime)samples[i].StartDate != endOfLastSample)
                            {
                                curLoc = 0;
                                curMinIn = 0;
                                
                            }
                            if (flattenSampleToVal(samples[i]) != curLoc % 2)
                            {
                                curLoc++;
                                curMinIn += ((DateTime)samples[i].EndDate - (DateTime)samples[i].StartDate).TotalSeconds / 60;
                            }
                            averages[curLoc] = (averages[curLoc] * sampleCounts[curLoc] + ((double)((DateTime)samples[i].EndDate - (DateTime)samples[i].StartDate).TotalSeconds / 60)) / sampleCounts[curLoc] + 1;
                            sampleCounts[curLoc]++;
                            
                        }
                    }
                }

            }
            lastEndDate = (DateTime)samples[0].EndDate;

        }

        public override DateTime EstimateWakeupTime()
        {
            string saveFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppData.json");
            string jsonstring = File.ReadAllText(saveFilename);
            AppData appData = JsonConvert.DeserializeObject<AppData>(jsonstring);
            DateTime wakeBy = appData.currDateTime();
            return lastEndDate.AddMinutes(DataBasedModel.findAlarmTime(wakeBy, averages, curLoc, curMinIn/*, lastEndDate*/)); //last end date is that last information we had
        }

        public override void LoadState()
        {
            string saveFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WatchData.json");
            string jsonstring = File.ReadAllText(saveFilename);
            AppleWatchDataMonitor hold = JsonConvert.DeserializeObject<AppleWatchDataMonitor>(jsonstring);
            this.averages = hold.averages;
            this.lastSample = hold.lastSample;
            this.sampleCounts = hold.sampleCounts;
            this.curLoc = hold.curLoc;
        }

        public override void SaveState()
        {
            string saveFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WatchData.json");
            
            string jsonstring;
            
            jsonstring = JsonConvert.SerializeObject(this);
            File.WriteAllText(saveFilename, jsonstring);
        }

        public override void ClearState()
        {
            for (int i = 0; i < sampleCounts.Length; i++)
            {
                sampleCounts[i] = 0;
                averages[i] = 0;
            }
            lastSample = DateTime.Now;
            curLoc = 0;
        }
    }

    
}