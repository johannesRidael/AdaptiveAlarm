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
using Utility;
using Fitbit.Api.Portable.Models;

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
        private DateTime datetimeLastCollected; // used to know whether or not to add a new index to the above arraylists
        private TimeSpan timeInCurrentState;    // represents the time between the start of the last recorded state and the time last recorded
        private int currentIndex;

        // new state vars
        private ArrayList avgs; // an array list of user states (wakeable / unwakeable) where odd indices are wakeable and even ones are unwakeable
        private ArrayList counts; // gives how many samples we have for each index in the above averages

        public FitbitDataMonitor()
        {
            FitbitAppCredentials credentials = new FitbitAppCredentials
            {
                ClientId = CLIENTID,
                ClientSecret = CLIENTSECRET
            };
            this.authHelper = new Fitbit.Api.Portable.OAuth2.OAuth2Helper(credentials, "http://localhost:4306/aa/data-collection");
            avgs = new ArrayList();
            counts = new ArrayList();
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

            // start rewrite here
            ArrayList sessionCycles = new ArrayList();
            if (new HashSet<string>() {"light", "wake" }.Contains(sleepSession.Levels.Data[0].Level))
            {
                sessionCycles.Add(0.0); // add an unwakeable value at index 0
            }
            // get the current session's cycle's as an array list of wakeable/unwakeable times in minutes
            var prevCycle = sleepSession.Levels.Data[0];
            foreach (var cycle in sleepSession.Levels.Data)
            { 
                switch(cycle.Level)
                {
                    case "rem":
                    case "deep":
                        if (prevCycle.Level.Equals("light") || prevCycle.Level.Equals("wake"))
                        {
                            sessionCycles.Add(cycle.Seconds / 60.0);
                        }
                        else
                        {
                            sessionCycles[sessionCycles.Count - 1] = ((double)sessionCycles[sessionCycles.Count - 1]) + cycle.Seconds / 60.0;
                        }
                        prevCycle = cycle;
                        break;
                    case "light":
                    case "wake":
                        if (prevCycle.Level.Equals("rem") || prevCycle.Level.Equals("deep"))
                        {
                            sessionCycles.Add(cycle.Seconds / 60.0);
                        }
                        else
                        {
                            sessionCycles[sessionCycles.Count - 1] = ((double)sessionCycles[sessionCycles.Count - 1]) + cycle.Seconds / 60.0;
                        }
                        prevCycle = cycle;
                        break;
                }
            }

            // ensure that counts and avgs are big enough
            if (avgs == null || counts == null)
                avgs = new ArrayList(); counts= new ArrayList();
            if (avgs.Count < sessionCycles.Count || counts.Count < sessionCycles.Count)
            {
                avgs.Capacity = sessionCycles.Count;
                counts.Capacity = sessionCycles.Count;
                for (int i = 0; i < sessionCycles.Count; i++)
                {
                    if (i >= avgs.Count)
                        avgs.Add(0.0);
                    if (i >= counts.Count)
                        counts.Add(0);
                }
            }

            int samples = (sleepSession.StartTime.Date - ((DateTime)Application.Current.Properties["CurrentDataSetStartDate"]).Date).Days + 1;

            // adjust averages and counts as appropriate
            for (int i = 0; i < sessionCycles.Count; i++)
            {
                if (i == sessionCycles.Count - 1) // don't add the last cycle value as it may still be happening.
                    continue;
                if ((int)counts[i] < samples)
                {
                    avgs[i] = (((double)avgs[i] * (samples - 1)) + (double)sessionCycles[i]) / samples;
                    counts[i] = (int)counts[i] + 1;
                }
            }


            datetimeLastCollected = DateTime.Now;
            timeInCurrentState = DateTime.Now - sleepSession.Levels.Data.Last<LevelsData>().DateTime;
            currentIndex = sessionCycles.Count - 1;
        }

        public override DateTime EstimateWakeupTime()
        {
            DateTime wakeBy = AppData.Load().currDateTime();
            
            // if we don't have any data, or the user is likely awake (next alarm day is different from the last day collected), just wake up at set time
            if (avgs == null || counts == null || avgs.Count == 0 || counts.Count == 0 || datetimeLastCollected.Day < wakeBy.Day)
                return wakeBy;

            return (datetimeLastCollected - timeInCurrentState).AddMinutes(DataBasedModel.findAlarmTime(wakeBy, avgs.Cast<double>().ToArray(), currentIndex, timeInCurrentState.TotalMinutes));
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

            Tuple<DateTime, TimeSpan, int> vals = JsonConvert.DeserializeObject<Tuple<DateTime, TimeSpan, int>>(valsString);
            this.datetimeLastCollected = vals.Item1;
            this.timeInCurrentState = vals.Item2;
            this.currentIndex = vals.Item3;

            // load in averages
            string avgsString = "";
            var avgsPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitAvgs.json");
            using (StreamReader streamReader = File.OpenText(avgsPath))
                avgsString = await streamReader.ReadToEndAsync();
            this.avgs = JsonConvert.DeserializeObject<ArrayList>(avgsString);

            // load in counts
            string countsString = "";
            var countsPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitCounts.json");
            using (StreamReader streamReader = File.OpenText(countsPath))
                countsString = await streamReader.ReadToEndAsync();
            this.avgs = JsonConvert.DeserializeObject<ArrayList>(countsString);

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
            string valsString = JsonConvert.SerializeObject(new Tuple<DateTime, TimeSpan, int>(this.datetimeLastCollected, this.timeInCurrentState, this.currentIndex));
            var valsPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitVals.json");
            using (StreamWriter streamWriter = File.CreateText(valsPath))
                await streamWriter.WriteAsync(valsString);

            // save averages
            string avgsString = JsonConvert.SerializeObject(this.avgs);
            var avgsPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitAvgs.json");
            using (StreamWriter streamWriter = File.CreateText(avgsPath))
                await streamWriter.WriteAsync(avgsString);

            // save counts
            string countsString = JsonConvert.SerializeObject(this.avgs);
            var countsPath = Path.Combine(FileSystem.AppDataDirectory, "FitbitCounts.json");
            using (StreamWriter streamWriter = File.CreateText(countsPath))
                await streamWriter.WriteAsync(countsString);

            //TODO: add code to save other state as it is added.
        }
        public override void ClearState()
        {
            SecureStorage.SetAsync("fitbit_tok", null);
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitVals.json"));
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitAvgs.json"));
            File.Delete(Path.Combine(FileSystem.AppDataDirectory, "FitbitCounts.json"));
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
