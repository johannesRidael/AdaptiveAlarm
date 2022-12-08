using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Adaptive_Alarm
{
    public class DataBasedModel
    {
        //public static int[] stages = { 3, 0, 1, 0, 2, 0, 1, 0, 2, 3, 0, 1, 0, 2, 0, 1, 0, 2, 0, 1, 0, 2, 0, 1, 0, 2, 0, 1, 0, 2, 0 }; //may remove or add awake sections

        public static double findAlarmTime(DateTime wakeBy, double[] avgTimes, int Location, double minInLocation)

        {
            /*
             * wakeBy: DateTime corresponding to when the user needs to wake.
             * avgTimes: an array where odd indices correspond to average time spent in "wakeable" stages (light/awake) and even indices to "unwakeable" (deep/REM)
             * Location: where the user currently is in the stages array above
             * minInLocation: how long the user has been in the current stage
             * 
             * Sleep cycle goes:
             * Light-Deep-Light-REM-Light-Deep-Light-REM-awake?-Light-Deep-Light-REM-Light-Deep-Light-REM-Light-Deep-Light-REM-Best Wakeup(after REM)
             * https://learn.chm.msu.edu/NeuroEd/neurobiology_disease/content/otheresources/sleepdisorders.pdf
             * 
             */


            TimeSpan ttSleep = wakeBy - DateTime.Now;
            double minToSleep = ttSleep.TotalMinutes;
            double timeTillAlarm = 0;

            int index = Location;


            double hold = minInLocation;
            hold += avgTimes[index];
            index++;
            if (index > avgTimes.Length)
                index -= 2;
            hold += avgTimes[index];

            if (Location % 2 == 1)
            {
                if (hold > minToSleep)
                {
                    timeTillAlarm = avgTimes[Location] / 2 - minInLocation;
                    if (timeTillAlarm < 0) { timeTillAlarm = 0; }
                    // if we don't have time to finish the current wakeable stage and the next unwakeable, we wake in the middle of this one
                    return timeTillAlarm;
                }
                else
                    timeTillAlarm = hold;
                index++;
                if (index > avgTimes.Length)
                    index -= 2;
            }
            else
                timeTillAlarm = avgTimes[Location] - minInLocation;

            //index is guaranteed odd
            index++;
            if (index > avgTimes.Length)
                index -= 2;
            //index-1 is guaranteed odd
            while (true)
            {
                if (timeTillAlarm + avgTimes[index - 1] + avgTimes[index] > minToSleep)
                {
                    if (timeTillAlarm + avgTimes[index - 1] / 2 > minToSleep)
                    {
                        return minToSleep; // if can't aim for middle, just take closest to middle allowed
                    }
                    else
                        return timeTillAlarm + avgTimes[index - 1] / 2; //aim for the middle of the stage if possible
                }
                timeTillAlarm += avgTimes[index - 1] + avgTimes[index];

                index++;
                if (index > avgTimes.Length)
                    index -= 2;
                index++;
                if (index > avgTimes.Length)
                    index -= 2;
            }
        }

        public static double findAlarmTime(DateTime wakeBy, int[] avgTimes, int Location, double minInLocation, DateTime startFrom)
        {
            /*
             * wakeBy: DateTime corresponding to when the user needs to wake.
             * avgTimes: an array where odd indices correspond to average time spent in "wakeable" stages (light/awake) and even indices to "unwakeable" (deep/REM)
             * Location: where the user currently is in the stages array above
             * minInLocation: how long the user has been in the current stage
             * 
             * Sleep cycle goes:
             * Light-Deep-Light-REM-Light-Deep-Light-REM-awake?-Light-Deep-Light-REM-Light-Deep-Light-REM-Light-Deep-Light-REM-Best Wakeup(after REM)
             * https://learn.chm.msu.edu/NeuroEd/neurobiology_disease/content/otheresources/sleepdisorders.pdf
             * 
             */




            TimeSpan ttSleep = wakeBy - startFrom;
            double minToSleep = ttSleep.TotalMinutes;
            double timeTillAlarm = 0;

            int index = Location;


            double hold = minInLocation;
            hold += avgTimes[index];
            index++;
            if (index > avgTimes.Length)
                index -= 2;
            hold += avgTimes[index];

            if (Location % 2 == 1)
            {
                if (hold > minToSleep)
                {
                    timeTillAlarm = avgTimes[Location] / 2 - minInLocation;
                    if (timeTillAlarm < 0) { timeTillAlarm = 0; }
                    // if we don't have time to finish the current wakeable stage and the next unwakeable, we wake in the middle of this one
                    return timeTillAlarm;
                }
                else
                    timeTillAlarm = hold;
                index++;
                if (index > avgTimes.Length)
                    index -= 2;
            }
            else
                timeTillAlarm = avgTimes[Location] - minInLocation;

            //index is guaranteed odd
            index++;
            if (index > avgTimes.Length)
                index -= 2;
            //index-1 is guaranteed odd
            while (true)
            {
                if (timeTillAlarm + avgTimes[index - 1] + avgTimes[index] > minToSleep)
                {
                    if (timeTillAlarm + avgTimes[index - 1] / 2 > minToSleep)
                    {
                        return minToSleep; // if can't aim for middle, just take closest to middle allowed
                    }
                    else
                        return timeTillAlarm + avgTimes[index - 1] / 2; //aim for the middle of the stage if possible
                }
                timeTillAlarm += avgTimes[index - 1] + avgTimes[index];

                index++;
                if (index > avgTimes.Length)
                    index -= 2;
                index++;
                if (index > avgTimes.Length)
                    index -= 2;
            }
        }
    }
}

