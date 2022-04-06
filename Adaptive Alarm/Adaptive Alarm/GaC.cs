using System;
using System.Collections.Generic;
using GaCData;
using Newtonsoft.Json;
using System.IO;

namespace Adaptive_Alarm
{
    internal class GaC
    {
        private DateTime nearestMidnight(DateTime dt)
        {
            DateTime p1 = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            DateTime p2 = new DateTime(dt.Year, dt.Month, dt.Day + 1, 0, 0, 0);
            double dif1 = Math.Abs((p1 - dt).TotalMinutes);
            double dif2 = Math.Abs((p2 - dt).TotalMinutes);
            if (dif1 < dif2)
            {
                return p1;
            }
            else
            {
                return p2;
            }
            //TODO: Add handling for nonconventional sleep times, i.e. graveyard workers

        }
        private int maxInd(int[] arr)
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

        protected int findAlarmTime(TimeSpan wakeBy, int awakeTime)
        {
            /*
             * wakeBy: TimeSpan for the correct day
             * awakeTime: user input amount of time they stay awake
             * 
             * returns: minutes until alarm should be set
             */
            string saveFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GaCData.json");
            int[] attempts;
            int[] scores;
            Queue<int> BFSqueue;
            int ind; // 0 for initial implementation
            int optCycleTime;
            int high;
            int low;
            int firstQScore;
            int[] searchTree = new int[] {0, 90, 75, 105, 67, 83, 97, 113, 63, 71, 79, 87, 93, 101, 109, 117, 61, 65, 69, 73, 77, 81, 85, 89, 91, 95, 99, 103, 107,
            111, 115, 119, 60, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 90, 92, 94, 96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118, 120};
            //searchTree is a BinarySearchTree represented as an array
            // if parent is at ind:
            //     left child is at ind*2
            //     right child is at ind*2 + 1
            //childs parent is at floor(ind/2)
            GaCDataContainer data;
            if (File.Exists(saveFilename))
            {
                string jsonstring = File.ReadAllText(saveFilename);
                data = JsonConvert.DeserializeObject<GaCDataContainer>(jsonstring);
            }
            else
            {
                data = new GaCDataContainer();
            }
            attempts = data.AttemptsArr;
            scores = data.ScoresArr;
            BFSqueue = data.BFSqueue;
            ind = data.ScoreAttemptTreeInd; // 0 for initial run
            optCycleTime = data.optCycleTime;
            high = data.upperBound;
            low = data.lowerBound;
            firstQScore = data.firstQscore;

            int CurrCycle = -1;
            if (optCycleTime == 0)
            {
                if (ind == 0)
                {//initialization
                    ind = 1;
                    CurrCycle = 90;
                }
                else if (low != 60 || high != 120)
                {//we have found a parent that scored better than both its children, so search between its two children
                    int diff = Math.Abs(attempts[ind-1] - attempts[ind]);
                    if (diff == 0)
                    {
                        int bestInd = maxInd(scores);
                        CurrCycle = attempts[bestInd];
                        optCycleTime = CurrCycle;
                    }
                    else if (scores[ind] > scores[ind-1])
                    {
                        attempts[ind+1] = attempts[ind] + (int)diff/2;
                        CurrCycle = attempts[ind+1];
                    }
                    else if (scores[ind] <= scores[ind-1])
                    {
                        attempts[ind+1] = attempts[ind-1] - diff;
                        while (Array.IndexOf(attempts, attempts[ind+1]) != ind + 1)
                        {
                            diff = (int)diff/2;
                            attempts[ind+1] = attempts[ind-1] - diff;
                            CurrCycle = attempts[ind+1];
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
                            ind = ind*2 + 1; //try right side first
                            CurrCycle = searchTree[ind];
                        }
                        BFSqueue.Clear();
                    }
                    else
                    {
                        if (ind * 2 < searchTree.Length)
                        {
                            BFSqueue.Enqueue(ind*2);
                            BFSqueue.Enqueue(ind*2 + 1);
                        }
                        ind = BFSqueue.Dequeue();
                        CurrCycle = searchTree[ind];
                    }
                }
                else
                {
                    if (scores[ind] > scores[(int)ind/2]) // if we scored better than our parent
                    {
                        if (ind * 2 + 1 >= searchTree.Length)
                        {//we've reached the end of the tree which means the best score we found should hopefully be the best possible

                            int bestInd = maxInd(scores);
                            CurrCycle = searchTree[bestInd];
                            optCycleTime = CurrCycle;
                        }
                        else
                        {
                            ind = ind*2 + 1; //try right side first
                            CurrCycle = searchTree[ind];
                        }
                    }
                    else if (scores[((int)ind/2) * 2] == 0)//we scored worse or equal to parent, and haven't tried left side yet
                    {
                        ind = ((int)ind/2) * 2;
                        CurrCycle = searchTree[ind];
                    }
                    else if (scores[ind] < scores[(int)ind/2]) // we scored worse than parent, and tried left side
                    {
                        low = scores[2 * (int)ind/2];
                        high = searchTree[1 + 2 * (int)ind/2];
                        attempts[0] = searchTree[(int)ind/2];
                        int hold = scores[(int)ind/2];
                        scores = new int[high - low + 1];
                        scores[0] = hold;
                        attempts[1] = low + (int)3*(high-low)/4;
                        CurrCycle = attempts[1];
                        ind = 1;
                    }
                    else //all three are equal, begin breadth first search
                    {
                        if (ind * 2 < searchTree.Length)
                        {
                            firstQScore = scores[ind];
                            BFSqueue.Enqueue((ind+1)*2); //add right sides children
                            BFSqueue.Enqueue(((ind+1)*2 + 1));
                            BFSqueue.Enqueue(ind*2);//add current children
                            BFSqueue.Enqueue(ind*2 + 1);
                            ind = BFSqueue.Dequeue();
                            CurrCycle = searchTree[ind];
                        }
                        else
                        {
                            optCycleTime = searchTree[(int)ind/2];
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
                data.ScoreAttemptTreeInd = ind;
                string jsonstring = JsonConvert.SerializeObject(data);
                File.WriteAllText(saveFilename, jsonstring);

            }
            else
            {
                CurrCycle = optCycleTime;
            }
            DateTime now = DateTime.Now;
            DateTime nm = nearestMidnight(now);
            DateTime wakeBy2 = nm + wakeBy;
            TimeSpan ttSleep = wakeBy2 - now;
            double minToSleep = ttSleep.TotalMinutes;

            int timeTillAlarm = awakeTime;
            while (timeTillAlarm < minToSleep - CurrCycle)
            {
                timeTillAlarm = timeTillAlarm + CurrCycle;
            }
            //timeTillAlarm is now how many minutes before we should wake the user
            return timeTillAlarm;
        }
    }
}
