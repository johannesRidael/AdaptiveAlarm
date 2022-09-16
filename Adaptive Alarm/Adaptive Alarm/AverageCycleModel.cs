using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Adaptive_Alarm
{
    internal class AverageCycleModel
    {
        public static int[] stages = { 3, 0, 1, 0, 2, 0, 1, 0, 2, 3, 0, 1, 0, 2, 0, 1, 0, 2, 0, 1, 0, 2, 0, 1, 0, 2, 0, 1, 0, 2, 0 }; //may remove or add awake sections

        public static int findAlarmTime(DateTime wakeBy, int[][] stageTimes, int Location, int minInLocation)
        {
            /*
             * wakeBy: DateTime corresponding to when the user needs to wake.
             * stageTimes: an array of 3/4 arrays where stageTimes[i][0] is the average time of the first time the user is in that stage,
             *                      stageTimes[i][1] the second, and so on. i=0 corresponds to Light Sleep, i=1 corresponds to Deep Sleep, i=2 to REM, i=3 to awake(if needed)
             * Location: where the user currently is in the stages array above
             * minInLocation: how long the user has been in the current stage
             * 
             * Sleep cycle goes:
             * Light-Deep-Light-REM-Light-Deep-Light-REM-awake?-Light-Deep-Light-REM-Light-Deep-Light-REM-Light-Deep-Light-REM-Best Wakeup(after REM)
             * https://learn.chm.msu.edu/NeuroEd/neurobiology_disease/content/otheresources/sleepdisorders.pdf
             * 
             * TODO: add handling for if we don't have enough time to reach the end of REM
             */
            int[] counts = { 0, 0, 0, 0 };
            int ourLoc;
            for (ourLoc = 0; ourLoc < Location; ourLoc++)
            {
                counts[stages[ourLoc]]++;
            }

            ArrayList nextCycles = new ArrayList();
            

            TimeSpan ttSleep = wakeBy - DateTime.Now;
            double minToSleep = ttSleep.TotalMinutes;
            int timeTillAlarm = 0;


            timeTillAlarm += stageTimes[stages[ourLoc]][counts[ourLoc]] - minInLocation;
            ourLoc++;
            counts[ourLoc]++;
            
            while (true)
            {
                int hold = 0;
                while (stages[ourLoc] != 2)
                {
                    hold += stageTimes[stages[ourLoc]][counts[ourLoc]];
                    ourLoc++;
                    counts[ourLoc]++;
                }
                hold += stageTimes[stages[ourLoc]][counts[ourLoc]];
                ourLoc++;
                counts[ourLoc]++; //hold should now be the time until the end of the next REM stage

                if (timeTillAlarm + hold <= minToSleep)
                {
                    timeTillAlarm += hold; //we only add time to sleep if we can while still ending right after REM
                }
                else
                {
                    break;
                }

            }
            //timeTillAlarm is now how many minutes before we should wake the user
            return timeTillAlarm;
        }
    }
}
