using Xamarin.Forms.Xaml;
using System.Collections.Generic;

namespace GaCData
{
    public class GaCDataContainer
    {
        public int[] AttemptsArr { get; set; }
        public int[] ScoresArr { get; set; }
        public int ScoreAttemptTreeInd { get; set; }
        public int optCycleTime { get; set; }
        public int upperBound { get; set; }
        public int lowerBound { get; set; }
        public Queue<int> BFSqueue { get; set; }
        public int firstQscore { get; set; }
        public int lastCycle { get; set; }

        public GaCDataContainer()
        {
            AttemptsArr = new int[64];
            ScoresArr = new int[64];
            BFSqueue = new Queue<int>();
            ScoreAttemptTreeInd = 0; // 0 for initial implementation
            optCycleTime = 0;
            upperBound = 120;
            lowerBound = 60;
            firstQscore = -1;
            lastCycle = 90;
        }
    }
}
