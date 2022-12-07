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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Adaptive_Alarm.Views;
using Adaptive_Alarm;
using System.Collections;
using System.Linq;

namespace DataMonitorLib
{
    public class FitbitDataMonitor : DataMonitor
    {
        // Auth Items
        private Fitbit.Api.Portable.OAuth2.OAuth2AccessToken token;
        private Fitbit.Api.Portable.OAuth2.OAuth2Helper authHelper;
        private const string CLIENTID = "238S3D";
        private const string CLIENTSECRET = "078679b0a1f8ad4b5d4aebba989571a7";

        // Data related to state
        private ArrayList remCycles = new ArrayList(); //2d arraylists first index is the day in question, the second
        private ArrayList deepCycles = new ArrayList(); //              is the number of cycle of that type on that day
        private ArrayList lightCycles = new ArrayList();  //            the value is the duration in seconds of that 
        private ArrayList wakeCycles = new ArrayList();  //             number of cycle of that type on that day.
        private DateTime startOfLastCollectedSession;
        private DateTime datetimeLastCollected; // used to know whether or not to add a new index to the above arraylists
        private string lastKnownCycleState;     // the current cycle at last collection

        public FitbitDataMonitor()
        {
            FitbitAppCredentials credentials = new FitbitAppCredentials
            {
                ClientId = CLIENTID,
                ClientSecret = CLIENTSECRET
            };
            this.authHelper = new Fitbit.Api.Portable.OAuth2.OAuth2Helper(credentials, "http://localhost:4306/aa/data-collection");
        }

        public override async void CollectDataPoint()
        {
            Console.WriteLine("FitbitDataMonitor - CollectDataPointRunning");
            // Build a Fitbit client and download the current day's sleep data.
            FitbitAppCredentials credentials = new FitbitAppCredentials();
            credentials.ClientId = CLIENTID;
            credentials.ClientSecret = CLIENTSECRET;

            FitbitClient client = new FitbitClient(credentials, this.token);
            Fitbit.Api.Portable.Models.SleepLogDateBase daysSleep = await client.GetSleepDateAsync(DateTime.Now);

            // Find the main sleep session/
            Fitbit.Api.Portable.Models.SleepLogDateRange sleepSession = null;
            foreach (Fitbit.Api.Portable.Models.SleepLogDateRange s in daysSleep.Sleep)
            {
                if (s.IsMainSleep) { sleepSession = s; }
            }

            if (sleepSession == null || sleepSession.StartTime + TimeSpan.FromMinutes(sleepSession.MinutesAwake + sleepSession.MinutesAsleep) < datetimeLastCollected)
            {
                return; // if there is no primary sleep session in the day or it has already been colllected then return with no update to internal data.
            }

            if (startOfLastCollectedSession.Date < sleepSession.StartTime.Date) // add a new index for the new day's data
            {
                remCycles.Add(new ArrayList(10));
                deepCycles.Add(new ArrayList(10));
                lightCycles.Add(new ArrayList(10));
                wakeCycles.Add(new ArrayList(10));
            }
            else if (startOfLastCollectedSession.Date == sleepSession.StartTime.Date) // set the last observed state if the user may be asleep
            {
                lastKnownCycleState = (sleepSession.Levels.Data.Last()).Level;
            }

            // add all of the current day's sleep data
            int index = (sleepSession.StartTime.Date - ((DateTime)Application.Current.Properties["CurrentDataSetStartDate"]).Date).Days;

            int r = 0, d = 0, l = 0, a = 0;
            foreach (var cycle in sleepSession.Levels.Data)
            {
                switch (cycle.Level)
                {
                    case "rem":
                        {
                            if (a < ((ArrayList)remCycles[index]).Count)
                            {
                                ((ArrayList)remCycles[index])[a] = cycle.Seconds;
                            }
                            else
                            {
                                ((ArrayList)remCycles[index]).Add(cycle.Seconds);
                            }
                            r++;
                            break;
                        }
                    case "deep":
                        {
                            if (a < ((ArrayList)deepCycles[index]).Count)
                            {
                                ((ArrayList)deepCycles[index])[a] = cycle.Seconds;
                            }
                            else
                            {
                                ((ArrayList)deepCycles[index]).Add(cycle.Seconds);
                            }
                            d++;
                            break;
                        }
                    case "light":
                        {
                            if (a < ((ArrayList)lightCycles[index]).Count)
                            {
                                ((ArrayList)lightCycles[index])[a] = cycle.Seconds;
                            }
                            else
                            {
                                ((ArrayList)lightCycles[index]).Add(cycle.Seconds);
                            }
                            l++;
                            break;
                        }
                    case "wake":
                        {
                            if (a < ((ArrayList)wakeCycles[index]).Count)
                            {
                                ((ArrayList)wakeCycles[index])[a] = cycle.Seconds;
                            }
                            else
                            {
                                ((ArrayList)wakeCycles[index]).Add(cycle.Seconds);
                            }
                            a++;
                            break;
                        }

                }
            }

            startOfLastCollectedSession = sleepSession.StartTime;
            datetimeLastCollected = DateTime.Now;
        }

        public override DateTime EstimateWakeupTime()
        {
            //TODO: Bring this in systematically
            DateTime wakeBy = DateTime.Today.AddDays(1).AddHours(8); //Next day's alarm time

            TimeSpan timeToSleep = wakeBy - DateTime.Now;
            double minutesToSleep = timeToSleep.TotalMinutes;

            int maxCycleCount = 10; //TODO: change so this isn't hard coded

            int[] avgRemDuration = new int[maxCycleCount];
            int[] avgDeepDuration = new int[maxCycleCount];
            int[] avgLightDuration = new int[maxCycleCount];
            int[] avgWakeDuration = new int[maxCycleCount];

            for ( int i = 0; i < maxCycleCount; i++) // total the duration for that position
            {

                foreach (var day in remCycles)
                {
                    if (i < ((ArrayList)day).Count)
                        avgRemDuration[i] += (int)((ArrayList)day)[i];
                }
                foreach (var day in deepCycles)
                {
                    if (i < ((ArrayList)day).Count)
                        avgDeepDuration[i] += (int)((ArrayList)day)[i];
                }
                foreach (var day in lightCycles)
                {
                    if (i < ((ArrayList)day).Count)
                        avgLightDuration[i] += (int)((ArrayList)day)[i];
                }
                foreach (var day in wakeCycles)
                {
                    if (i < ((ArrayList)day).Count)
                        avgWakeDuration[i] += (int)((ArrayList)day)[i];
                }
                // calculate that position's average duration
                avgRemDuration[i] = (int)avgRemDuration[i] / remCycles.Count;
                avgDeepDuration[i] = (int)avgDeepDuration[i] / remCycles.Count;
                avgLightDuration[i] = (int)avgLightDuration[i] / remCycles.Count;
                avgWakeDuration[i] = (int)avgWakeDuration[i] / remCycles.Count;
            }

            DateTime start = startOfLastCollectedSession; // we will assume they went/will go to sleep at the last time we saw them go to sleep

            int currentDaysIndex = (startOfLastCollectedSession.Date - ((DateTime)Application.Current.Properties["CurrentDataSetStartDate"]).Date).Days;
            int rIndex = ((ArrayList)remCycles[currentDaysIndex]).Count;
            int dIndex = ((ArrayList)deepCycles[currentDaysIndex]).Count;
            int lIndex = ((ArrayList)lightCycles[currentDaysIndex]).Count;
            int wIndex = ((ArrayList)wakeCycles[currentDaysIndex]).Count;

            List<string> options = new List<string>() { "rem", "deep", "light", "wake" };
            var random = new Random();
            string lastType = lastKnownCycleState;
            bool foundResult = false;

            DateTime result = startOfLastCollectedSession; // as it stands this reflects the last anticipated state transition before they must be up
            while (result < wakeBy && !foundResult)
            {
                string s = options[random.Next(options.Count)]; // assume random state transitions (we don't track this at the moment)
                if (s.Equals(lastType))
                    continue;
                switch (s)
                {
                    case "rem":
                        if (result + TimeSpan.FromSeconds(avgRemDuration[rIndex]) < wakeBy)
                        {
                            result += TimeSpan.FromSeconds(avgRemDuration[rIndex]);
                            rIndex++;
                            break;
                        }
                        foundResult = true;
                        break;
                    case "light":
                        if (result + TimeSpan.FromSeconds(avgLightDuration[lIndex]) < wakeBy)
                        {
                            result += TimeSpan.FromSeconds(avgLightDuration[lIndex]);
                            lIndex++;
                            break;
                        }
                        foundResult = true;
                        break;
                    case "deep":
                        if (result + TimeSpan.FromSeconds(avgDeepDuration[dIndex]) < wakeBy)
                        {
                            result += TimeSpan.FromSeconds(avgDeepDuration[dIndex]);
                            dIndex++;
                            break;
                        }
                        foundResult = true;
                        break;
                    case "wake":
                        if (result + TimeSpan.FromSeconds(avgWakeDuration[wIndex]) < wakeBy)
                        {
                            result += TimeSpan.FromSeconds(avgWakeDuration[wIndex]);
                            wIndex++;
                            break;
                        }
                        foundResult = true;
                        break;
                }
            }


            return result;
        }

        public override async void LoadState()
        {
            string potential_tok = await SecureStorage.GetAsync("fitbit_tok");
            if (potential_tok != null)
            {
                try
                {
                    this.token = JsonConvert.DeserializeObject<Fitbit.Api.Portable.OAuth2.OAuth2AccessToken>(potential_tok);
                }
                catch (JsonException)
                {
                    Console.WriteLine("There was an error loading a previous Fitbit API Token, reauthenticating");
                    this.Authenticate();
                    return;
                }
            }
            else // there is no auth token saved to the device, reauthenticate
            {
                Console.WriteLine("No token found, authenticating");
                this.Authenticate();
                return;
            }

            // Load in previous loose values
            string valsString = "";
            var valsPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitVals.json");
            using (StreamReader streamReader = File.OpenText(valsPath))
                valsString = await streamReader.ReadToEndAsync();

            Tuple<DateTime, DateTime, string> vals = JsonConvert.DeserializeObject<Tuple<DateTime, DateTime, string>>(valsString);
            this.startOfLastCollectedSession = vals.Item1;
            this.datetimeLastCollected = vals.Item2;
            this.lastKnownCycleState = vals.Item3;

            // load in rem cycles
            string remCyclesString = "";
            var remCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitRemCycles.json");
            using (StreamReader streamReader = File.OpenText(remCyclesPath))
                remCyclesString = await streamReader.ReadToEndAsync();
            this.remCycles = JsonConvert.DeserializeObject<ArrayList>(remCyclesString);

            // load in deep cycles
            string deepCyclesString = "";
            var deepCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitDeepCycles.json");
            using (StreamReader streamReader = File.OpenText(deepCyclesPath))
                deepCyclesString = await streamReader.ReadToEndAsync();
            this.deepCycles = JsonConvert.DeserializeObject<ArrayList>(deepCyclesString);

            // load in light cycles
            string lightCyclesString = "";
            var lightCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitLightCycles.json");
            using (StreamReader streamReader = File.OpenText(lightCyclesPath))
                lightCyclesString = await streamReader.ReadToEndAsync();
            this.lightCycles = JsonConvert.DeserializeObject<ArrayList>(lightCyclesString);

            // load in wake cycles
            string wakeCyclesString = "";
            var wakeCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitWakeCycles.json");
            using (StreamReader streamReader = File.OpenText(wakeCyclesPath))
                wakeCyclesString = await streamReader.ReadToEndAsync();
            this.wakeCycles = JsonConvert.DeserializeObject<ArrayList>(wakeCyclesString);

            //TODO: Add code to load other state information as it is added
        }


        public override async void SaveState()
        {
            // sets the token's UtcExpirationDate so that when we load it in we can use token.IsFresh to see if we need to refresh.
            DateTime exp_time = DateTime.UtcNow + TimeSpan.FromSeconds(this.token.ExpiresIn);
            this.token.UtcExpirationDate = exp_time;

            string tok_s = JsonConvert.SerializeObject(this.token);
            //Console.WriteLine(tok_s);

            // store encrypted in the devices secure store.
            await SecureStorage.SetAsync("fitbit_tok", tok_s);

            // Save current loose values
            string valsString = JsonConvert.SerializeObject(new Tuple<DateTime, DateTime, string>(this.startOfLastCollectedSession, this.datetimeLastCollected, this.lastKnownCycleState));
            var valsPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitVals.json");
            using (StreamWriter streamWriter = File.CreateText(valsPath))
                await streamWriter.WriteAsync(valsString);

            // save rem cycles
            string remCyclesString = JsonConvert.SerializeObject(this.remCycles);
            var remCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitRemCycles.json");
            using (StreamWriter streamWriter = File.CreateText(remCyclesPath))
                await streamWriter.WriteAsync(remCyclesString);

            // save deep cycles
            string deepCyclesString = JsonConvert.SerializeObject(this.deepCycles);
            var deepCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitDeepCycles.json");
            using (StreamWriter streamWriter = File.CreateText(deepCyclesPath))
                await streamWriter.WriteAsync(deepCyclesString);

            // save light cycles
            string lightCyclesString = JsonConvert.SerializeObject(this.lightCycles);
            var lightCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitLightCycles.json");
            using (StreamWriter streamWriter = File.CreateText(lightCyclesPath))
                await streamWriter.WriteAsync(lightCyclesString);

            // save wake cycles
            string wakeCyclesString = JsonConvert.SerializeObject(this.wakeCycles);
            var wakeCyclesPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitWakeCycles.json");
            using (StreamWriter streamWriter = File.CreateText(wakeCyclesPath))
                await streamWriter.WriteAsync(wakeCyclesString);

            //TODO: add code to save other state as it is added.
        }
        public override void ClearState()
        {
            SecureStorage.SetAsync("fitbit_tok", null);
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitVals.json"));
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitRemCycles.json"));
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitDeepCycles.json"));
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitLightCycles.json"));
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitWakeCycles.json"));
        }

        /// <summary>
        /// Attempts to authenticate the Fitbit DataMonitor and set its Access and Refresh Tokens to new values.
        /// </summary>
        public void Authenticate()
        {
            // build authentication url and open it in the default web browser
            string authUrl = this.authHelper.GenerateAuthUrl(new string[] { "sleep", "heartrate" }, null);

            // open a browser to authenticate w/ Fitbit.
            Shell.Current.GoToAsync("app://shell/LoginPage");
            Console.WriteLine("Attempted to open LoginPage");
            // While the loginPage control is being focused it will determine if the current device setting is for a fitbit, if it is
            // it will call the below get auth url method and redirect there. That control will also be set to on redirect check the
            // redirect url, if it begins 'https://localhost:4306/aa/data-collection' it will strip out the auth code and exchange it
            // for a token set setting that value on our data monitor.
        }

        public string GetAuthUrl()
        {
            // build authentication url and open it in the default web browser
            string authUrl = this.authHelper.GenerateAuthUrl(new string[] { "sleep"}, null);
            return authUrl;
        }

        public async void GetToken(string code)
        {
            Console.WriteLine("Auth Code: " + code);

            this.token = await this.authHelper.ExchangeAuthCodeForAccessTokenAsync(code);
            Console.WriteLine("Token: " + this.token.Token);


            await Shell.Current.GoToAsync("app://shell/SettingsPage");
            Console.WriteLine("Attempted to open Settings Page after Fitbit Token retrieved");

            // save the datamonitor's state in case of crash.
            this.SaveState();
        }
    }
}
