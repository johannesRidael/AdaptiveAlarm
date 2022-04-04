﻿using Xamarin.Forms;
using System;
using System.Collections.Generic;


namespace Adaptive_Alarm
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

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
            for(int i = 1; i < arr.Length; i++)
            {
                if (arr[i] > max)
                {
                    ret = i;
                    max = arr[i];
                }
            }
            return ret;
        }

        int[] attempts = new int[64];
        int[] scores = new int[64];
        int ind = 0; // 0 for initial implementation
        int trialC = 0;
        int optCycleTime = 0;
        int awakeTime = 10; // in minutes
        int high = 120;
        int low = 60;
        int[] searchTree = {0, 90, 75, 105, 67, 83, 97, 113, 63, 71, 79, 87, 93, 101, 109, 117, 61, 65, 69, 73, 77, 81, 85, 89, 91, 95, 99, 103, 107, 
            111, 115, 119, 60, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 90, 92, 94, 96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118, 120};
        protected async void findAlarmTime()
        {
            //searchTree is a BinarySearchTree represented as an array
            // if parent is at ind:
            //     left child is at ind*2
            //     right child is at ind*2 + 1
            //childs parent is at floor(ind/2)
            int CurrCycle = -1;
            if(optCycleTime == 0)
            {
                if(ind == 0)
                {
                    ind = 1;
                    CurrCycle = 90; 
                }
                else if (low != 60 || high != 120)
                {
                    int diff = Math.Abs(attempts[ind-1] - attempts[ind]);
                    if(diff == 0)
                    {
                        int bestInd = maxInd(scores);
                        CurrCycle = attempts[bestInd];
                        optCycleTime = CurrCycle;
                    }
                    else if (scores[ind] > scores[ind-1])
                    {
                        attempts[ind+1] = attempts[ind] + (int) diff/2;
                    }
                    else if (scores[ind] <= scores[ind-1])
                    {
                        attempts[ind+1] = attempts[ind-1] - diff;
                        while (Array.IndexOf(attempts, attempts[ind+1]) != ind + 1)
                        {
                            diff = (int)diff/2;
                            attempts[ind+1] = attempts[ind-1] - diff;
                        }
                    }
                    CurrCycle = attempts[ind+1];
                    ind++;

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
                        scores = new int[high - low + 1];
                        scores[0] = scores[(int)ind/2];
                        attempts[1] = low + (int)3*(high-low)/4;
                        CurrCycle = attempts[1];
                        ind = 1;
                    }
                    else //all three are equal
                    {

                    }
                }
            }
            else
            {
                CurrCycle = optCycleTime;
            }
            DateTime now = DateTime.Now;
            TimeSpan wakeBy = getWakeBy();
            DateTime nm = nearestMidnight(now);
            DateTime wakeBy2 = nm + wakeBy;
            TimeSpan ttSleep = wakeBy2 - now;
            double minToSleep = ttSleep.TotalMinutes;

            int timeTillAlarm = awakeTime;
            while(timeTillAlarm < minToSleep - CurrCycle)
            {
                timeTillAlarm = timeTillAlarm + CurrCycle;
            }
            //timeTillAlarm is now how many minutes before we should wake the user
        }
    }
}
