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

namespace DataMonitorLib
{
    public class FitbitDataMonitor : DataMonitor
    {
        private Fitbit.Api.Portable.OAuth2.OAuth2AccessToken token;
        private Fitbit.Api.Portable.OAuth2.OAuth2Helper authHelper;
        // Temporary ClientId and Secret for use in testing.
        private const string CLIENTID = "2388MW";
        private const string CLIENTSECRET = "778c9cc1bd4c44842e876cef351ec899";
        private Dictionary<String, ValueTuple<int, int>> avgStateDurations; // represents sleep states as a dictionary of (occurances, avg duration) tuples

        public FitbitDataMonitor()
        {
            FitbitAppCredentials credentials = new FitbitAppCredentials();
            credentials.ClientId = CLIENTID;
            credentials.ClientSecret = CLIENTSECRET;
            this.authHelper = new Fitbit.Api.Portable.OAuth2.OAuth2Helper(credentials, "http://localhost:4306/aa/data-collection");
        }

        public override async void CollectDataPoint()
        {
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

            if (sleepSession == null)
            {
                return; // if there is no primary sleep session in the day then return with no update to internal data.
            }

            if (DateTime.Now.TimeOfDay > sleepSession.StartTime.AddHours(8).TimeOfDay) // if the data point is being collected more than 8 hrs after the start of the day's sleep then we can assume that the person is awake and update the model
            {
                Fitbit.Api.Portable.Models.SummaryOfSleepTypes typeSummaries = sleepSession.Levels.Summary;
                avgStateDurations["Deep"] = (avgStateDurations["Deep"].Item1 + 1, ((avgStateDurations["Deep"].Item2 * avgStateDurations["Deep"].Item1) + (typeSummaries.Deep.Minutes / typeSummaries.Deep.Count)) / (avgStateDurations["Deep"].Item1 + 1));
                avgStateDurations["Light"] = (avgStateDurations["Light"].Item1 + 1, ((avgStateDurations["Light"].Item2 * avgStateDurations["Light"].Item1) + (typeSummaries.Deep.Minutes / typeSummaries.Deep.Count)) / (avgStateDurations["Light"].Item1 + 1));
                avgStateDurations["Rem"] = (avgStateDurations["Rem"].Item1 + 1, ((avgStateDurations["Rem"].Item2 * avgStateDurations["Rem"].Item1) + (typeSummaries.Deep.Minutes / typeSummaries.Deep.Count)) / (avgStateDurations["Rem"].Item1 + 1));
                avgStateDurations["Wake"] = (avgStateDurations["Wake"].Item1 + 1, ((avgStateDurations["Wake"].Item2 * avgStateDurations["Wake"].Item1) + (typeSummaries.Deep.Minutes / typeSummaries.Deep.Count)) / (avgStateDurations["Wake"].Item1 + 1));
            }
            else // we assume that they are asleep and attempt to align our internal model with their current sleep state.
            {
                Fitbit.Api.Portable.Models.LevelsData[] d = sleepSession.Levels.Data;
                Fitbit.Api.Portable.Models.LevelsData currentState = d[d.Length - 1];
                Fitbit.Api.Portable.Models.LevelsData lastState = d[d.Length - 2];
                Fitbit.Api.Portable.Models.LevelsData secondLastState = d[d.Length - 3];

                if (currentState.Level == "wake") { /* use lastState and secondLastState */}
                else if (lastState.Level == "wake") { /* use currentState and secondLastState */}
                else { /* use currentState and lastState*/}

                throw new NotImplementedException();
            }
        }

        public override DateTime EstimateWakeupTime()
        {
            throw new NotImplementedException();
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

            // Load in previous sleep state durations from the app data directory
            string text = "";
            var path = Path.Combine(FileSystem.AppDataDirectory, "FitbitStateDurations.json");
            using (StreamReader streamReader = File.OpenText(path))
                text = await streamReader.ReadToEndAsync();

            this.avgStateDurations = JsonConvert.DeserializeObject<Dictionary<string, ValueTuple<int, int>>>(text);

            //TODO: Add code to load other state information as it is added
        }


        public override async void SaveState()
        {
            // sets the token's UtcExpirationDate so that when we load it in we can use token.IsFresh to see if we need to refresh.
            DateTime exp_time = DateTime.UtcNow + TimeSpan.FromSeconds(this.token.ExpiresIn);
            this.token.UtcExpirationDate = exp_time;

            string tok_s = JsonConvert.SerializeObject(this.token);
            Console.WriteLine(tok_s);

            // store encrypted in the devices secure store.
            await SecureStorage.SetAsync("fitbit_tok", tok_s);

            // Save current sleep state durations to the app data directory
            string text = JsonConvert.SerializeObject(this.avgStateDurations);

            var path = Path.Combine(FileSystem.AppDataDirectory, "FitbitStateDurations.json");
            using (StreamWriter streamWriter = File.CreateText(path))
                await streamWriter.WriteAsync(text);

            //TODO: add code to save other state as it is added.
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
            string authUrl = this.authHelper.GenerateAuthUrl(new string[] { "sleep", "heartrate" }, null);
            return authUrl;
        }

        public async void GetToken(string code) //TODO: change this to return a bool indicating success or failure
        {
            Console.WriteLine("Auth Code: " + code);

            this.token = await this.authHelper.ExchangeAuthCodeForAccessTokenAsync(code);
            Console.WriteLine("Token: " + this.token.Token);


            await Shell.Current.GoToAsync("app://shell/SettingsPage");
            Console.WriteLine("Attempted to open Settings Page");

            // save the datamonitor's state in case of crash.
            this.SaveState();
        }

        public override void ClearState()
        {
            throw new NotImplementedException();
        }
    }
}
