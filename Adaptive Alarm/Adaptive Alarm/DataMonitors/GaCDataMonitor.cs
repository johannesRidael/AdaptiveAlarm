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
using GaCData;
using Xamarin.Essentials;
using Xamarin.Forms;
using Utility;

namespace DataMonitorLib
{
    public class GaCDataMonitor : DataMonitor
    {
        private static string saveFilename = Path.Combine(FileSystem.AppDataDirectory, "GaCData.json");
        private GaCDataContainer data;
        private static string appDataSaveFilename = Path.Combine(FileSystem.AppDataDirectory, "AppData.json");
        private AppData appData;

        public override void ClearState()
        {
            File.Delete(saveFilename);
            this.data = null;
        }

        public override void CollectDataPoint()
        {
            Console.WriteLine($"GaCDataMonitor - CollectDataPointRunning. Is Visible {Application.Current.Properties["isInForeground"]}");

            appData = AppData.Load();
            DayOfWeek today = DateTime.Now.DayOfWeek;
            if (Math.Abs((DateTime.Now.TimeOfDay - appData.GetTimeSpan(today)).TotalMinutes) < 120)
            {
                Application.Current.Properties["gacNeedsNewData"] = true; // if we are within two hours of the alarm going off set it so that we get a score prompt on the main screen
            }
            else
            {
                Application.Current.Properties["gacNeedsNewData"] = false; // otherwise we don't want a score prompt
            }
        }

        public async void PromptForData()
        {
            HashSet<string> acceptableScores = new HashSet<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            string result = await Shell.Current.DisplayPromptAsync("Wakefulness", "How rested did you feel waking up this morning?", placeholder: "Scale 1-10 where 10 is best", maxLength: 2, keyboard: Keyboard.Numeric);

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                while (!acceptableScores.Contains(result))
                {
                    result = await Shell.Current.DisplayPromptAsync("Wakefulness", "Please input a number 1-10", placeholder: "Scale 1-10 where 10 is best", maxLength: 2, keyboard: Keyboard.Numeric);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        result = result.Trim();
                    }
                }
                int score = Convert.ToInt32(result);
                this.data.ScoresArr[this.data.ScoreAttemptTreeInd] = score;
                
                appData = AppData.Load();
                appData.scoreAdded = DateTime.Now;
                appData.Save();

                Application.Current.Properties["gacHasNewData"] = true;
                Application.Current.Properties["gacNeedsNewData"] = false;
            }
        }

        public override DateTime EstimateWakeupTime()
        {
            if ((bool)(Application.Current.Properties["gacHasNewData"]))
            {
                //TODO: Bring these in systematically
                appData = AppData.Load();

                DateTime wakeBy = appData.currDateTime();
                int awakeTime = 20; // time spent awake

                int[] attempts;
                int[] scores;
                Queue<int> BFSqueue;
                int ind; // 0 for initial implementation
                int optCycleTime;
                int high;
                int low;
                int firstQScore;
                int lastCycle;
                int[] searchTree = new int[] {0, 90, 75, 105, 67, 83, 97, 113, 63, 71, 79, 87, 93, 101, 109, 117, 61, 65, 69, 73, 77, 81, 85, 89, 91, 95, 99, 103, 107,
            111, 115, 119, 60, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 90, 92, 94, 96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118, 120};

                attempts = data.AttemptsArr;
                scores = data.ScoresArr;
                BFSqueue = data.BFSqueue;
                ind = data.ScoreAttemptTreeInd; // 0 for initial run
                optCycleTime = data.optCycleTime;
                high = data.upperBound;
                low = data.lowerBound;
                firstQScore = data.firstQscore;
                lastCycle = data.lastCycle;

                int CurrCycle = 90;
                if (optCycleTime == 0)
                {
                    if (ind == 0)
                    {//initialization
                        ind = 1;
                        CurrCycle = 90;

                    }
                    else if (scores[ind] == 0)
                    {
                        //no score was recorded, use last value
                        CurrCycle = lastCycle;
                    }
                    else if (low != 60 || high != 120)
                    {//we have found a parent that scored better than both its children, so search between its two children
                        int diff = Math.Abs(attempts[ind - 1] - attempts[ind]);
                        if (diff == 0)
                        {
                            int bestInd = maxInd(scores);
                            CurrCycle = attempts[bestInd];
                            optCycleTime = CurrCycle;
                        }
                        else if (scores[ind] > scores[ind - 1])
                        {
                            attempts[ind + 1] = attempts[ind] + (int)diff / 2;
                            CurrCycle = attempts[ind + 1];
                        }
                        else if (scores[ind] <= scores[ind - 1])
                        {
                            attempts[ind + 1] = attempts[ind - 1] - diff;
                            while (Array.IndexOf(attempts, attempts[ind + 1]) != ind + 1)
                            {
                                diff = (int)diff / 2;
                                attempts[ind + 1] = attempts[ind - 1] - diff;
                                CurrCycle = attempts[ind + 1];
                            }
                        }

                        ind++;

                    }
                    else if (BFSqueue.Count > 0)
                    {//We have not made progress and so enter BFS mode
                        if (scores[ind] > firstQScore)
                        {//exit BFS mode
                            if (ind * 2 + 1 >= searchTree.Length)
                            {//we've reached the end of the tree which means the best score we found should hopefully be the best possible

                                int bestInd = maxInd(scores);
                                CurrCycle = searchTree[bestInd];
                                optCycleTime = CurrCycle;
                            }
                            else
                            {
                                ind = ind * 2 + 1; //try right side first
                                CurrCycle = searchTree[ind];
                            }
                            BFSqueue.Clear();
                        }
                        else
                        {
                            if (ind * 2 < searchTree.Length)
                            {
                                BFSqueue.Enqueue(ind * 2);
                                BFSqueue.Enqueue(ind * 2 + 1);
                            }
                            ind = BFSqueue.Dequeue();
                            CurrCycle = searchTree[ind];
                        }
                    }
                    else
                    {
                        if (scores[ind] > scores[(int)ind / 2]) // if we scored better than our parent
                        {
                            if (ind * 2 + 1 >= searchTree.Length)
                            {//we've reached the end of the tree which means the best score we found should hopefully be the best possible

                                int bestInd = maxInd(scores);
                                CurrCycle = searchTree[bestInd];
                                optCycleTime = CurrCycle;
                            }
                            else
                            {
                                ind = ind * 2 + 1; //try right side first
                                CurrCycle = searchTree[ind];
                            }
                        }
                        else if (scores[((int)ind / 2) * 2] == 0)//we scored worse or equal to parent, and haven't tried left side yet
                        {
                            ind = ((int)ind / 2) * 2;
                            CurrCycle = searchTree[ind];
                        }
                        else if (scores[ind] < scores[(int)ind / 2]) // we scored worse than parent, and tried left side
                        {
                            low = scores[2 * (int)ind / 2];
                            high = searchTree[1 + 2 * (int)ind / 2];
                            attempts[0] = searchTree[(int)ind / 2];
                            int hold = scores[(int)ind / 2];
                            scores = new int[high - low + 1];
                            scores[0] = hold;
                            attempts[1] = low + (int)3 * (high - low) / 4;
                            CurrCycle = attempts[1];
                            ind = 1;
                        }
                        else //all three are equal, begin breadth first search
                        {
                            if (ind * 2 < searchTree.Length)
                            {
                                firstQScore = scores[ind];
                                BFSqueue.Enqueue((ind + 1) * 2); //add right sides children
                                BFSqueue.Enqueue(((ind + 1) * 2 + 1));
                                BFSqueue.Enqueue(ind * 2);//add current children
                                BFSqueue.Enqueue(ind * 2 + 1);
                                ind = BFSqueue.Dequeue();
                                CurrCycle = searchTree[ind];
                            }
                            else
                            {
                                optCycleTime = searchTree[(int)ind / 2];
                                CurrCycle = optCycleTime;
                            }
                        }
                    }
                    data.firstQscore = firstQScore;
                    data.AttemptsArr = attempts;
                    data.ScoresArr = scores;
                    data.optCycleTime = optCycleTime;
                    data.upperBound = high;
                    data.lowerBound = low;
                    data.BFSqueue = BFSqueue;
                    data.lastCycle = CurrCycle;
                    data.ScoreAttemptTreeInd = ind;
                    string jsonstring = JsonConvert.SerializeObject(data);
                    File.WriteAllText(saveFilename, jsonstring);

                }
                else
                {
                    CurrCycle = optCycleTime;
                }

                TimeSpan ttSleep = wakeBy - DateTime.Now;
                double minToSleep = ttSleep.TotalMinutes;

                int timeTillAlarm = awakeTime;
                while (timeTillAlarm < minToSleep - CurrCycle)
                {
                    timeTillAlarm = timeTillAlarm + CurrCycle;
                }

                Application.Current.Properties["gacHasNewData"] = false;

                //timeTillAlarm is now how many minutes before we should wake the user
                DateTime wakeTime = DateTime.Now.AddMinutes(timeTillAlarm);
                return wakeTime;
            }
            return AppData.Load().currDateTime();
        }

        public override void LoadState()
        {
            //If a prior state was saved locally then load it in.
            if (File.Exists(saveFilename))
            {
                string jsonstring = File.ReadAllText(saveFilename);
                this.data = JsonConvert.DeserializeObject<GaCDataContainer>(jsonstring);
            }
            else
            {
                this.data = new GaCDataContainer();
            }
            // store these for use in determining if we needs to collect / process new data
            if (!Application.Current.Properties.ContainsKey("gacHasNewData"))
                Application.Current.Properties["gacHasNewData"] = false;
            if (!Application.Current.Properties.ContainsKey("gacNeedsNewData"))
                Application.Current.Properties["gacNeedsNewData"] = true;
        }

        public override void SaveState()
        {
            string jsonstrin = JsonConvert.SerializeObject(this.data);
            File.WriteAllText(saveFilename, jsonstrin);
        }

        private static int maxInd(int[] arr)
        {
            int ret = -1;
            int max = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] > max)
                {
                    ret = i;
                    max = arr[i];
                }
            }
            return ret;
        }
    }
}
