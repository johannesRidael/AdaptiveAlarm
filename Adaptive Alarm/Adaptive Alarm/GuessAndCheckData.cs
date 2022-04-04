using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)] 
namespace AppData
{
    public abstract class AppDataContainer
    {
        public int[] AttemptsArr { get; set; }
        public int[] ScoresArr { get; set; }
        public int ScoreAttemptTreeInd { get; set; }
        public int optCycleTime { get; set; }
        public int upperBound { get; set; }
        public int lowerBound { get; set; }

    }
}