using Xamarin.Forms;
using System;
using System.Collections.Generic

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

        private void nearestMidnight(DateTime dt)
        {
            DateTime p1 = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            DateTime p2 = new DateTime(dt.Year, dt.Month, dt.Day + 1, 0, 0, 0);
            TimeSpan dif1 = Math.Abs(p1 - dt);
            TimeSpan dif2 = Math.Abs(p2 - dt);
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

        int[] attempts = new int[60];
        int[] scores = new int[60];
        int ind = 0;
        int optCycleTime = 0;
        int awakeTime = 10; // in minutes
        int step = 15;
        protected async void findAlarmTime()
        {
            int CurrCycle = -1;
            if(optCycleTime == 0)
            {
                // use step and prev to find next attempt
                if (ind == 0)
                {
                    attempts[ind] = 90;
                    CurrCycle = 90;
                }
                else if (ind == 1)
                {
                    attempts[ind] = 105;
                    CurrCycle = 105;
                }
                else if (ind == 2)
                {

                }
                else if (ind == 3 && scores[0] == scores[1] && scores[1] == scores[2])
                {
                    attempts[ind] = 60;
                    CurrCycle = 60;
                }
                else if (ind == 4 && scores[0] == scores[1] && scores[1] == scores[2] && scores[2] == scores[3])
                {
                    attempts[ind] = 120;
                    CurrCycle = 120;
                }
                else
                {
                    int maxScore = 0;
                    int maxInd = -1;
                    HashSet<int> attempts = new HashSet<int>();
                    for(int i = 1; i < 4; i++)
                    {
                        attempts.Add(attempts[ind-i]);
                        if(scores[ind-i] > maxScore)
                        {
                            maxScore = scores[ind-i];
                            maxInd = i;
                        }
                    }
                    int diff = Math.Max(Math.Abs(attempts[ind-1] - attempts[ind-2]), Math.Abs(attempts[ind-2] - attempts[ind-3]) / 2;
                    attempts[ind] = attempts[ind-maxInd];
                    
                    CurrCycle = attempts[ind-maxInd] + diff;
                    if (attempts.Contains(CurrCycle))
                    {
                        CurrCycle -= 2*diff;
                    }
                    attempts[ind+1] = CurrCycle
                }
            }
            else
            {
                CurrCycle = optCycleTime;
            }
            DateTime now = DateTime.Now;
            TimeSpan wakeBy = getWakeBy();
            DateTime nm = nearestMidnight(now);
            DateTime wakeBy = nm + wakeBy;
            TimeSpan ttSleep = wakeBy - now;
            int minToSleep = ttSleep.TotalMinutes;

            int timeTillAlarm = awakeTime;
            while(timeTillAlarm < minToSleep - CurrCycle)
            {
                timeTillAlarm = timeTillAlarm + CurrCycle;
            }
            //timeTillAlarm is now how many minutes before we should wake the user
        }
    }
}
