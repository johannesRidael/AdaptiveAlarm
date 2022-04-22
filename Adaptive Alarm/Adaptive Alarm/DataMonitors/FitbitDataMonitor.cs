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

        public FitbitDataMonitor()
        {
            FitbitAppCredentials credentials = new FitbitAppCredentials();
            credentials.ClientId = CLIENTID;
            credentials.ClientSecret = CLIENTSECRET;
            this.authHelper = new Fitbit.Api.Portable.OAuth2.OAuth2Helper(credentials, "http://localhost:4306/aa/data-collection");
        }

        public override void CollectDataPoint()
        {
            // this is represented in lines 54- of the testing app.
            throw new NotImplementedException();
        }

        public override DateTime EstimateWakeupTime()
        {
            throw new NotImplementedException();
        }

        public override List<DateTime> GetSleepSessionCollectionTimes()
        {
            throw new NotImplementedException();
        }

        public override async void LoadState()
        {
            //TODO: Add code to load other state information as it is added
            string potential_tok = await SecureStorage.GetAsync("fitbit_tok");
            if (potential_tok != null)
            {
                try
                {
                    this.token = JsonConvert.DeserializeObject<Fitbit.Api.Portable.OAuth2.OAuth2AccessToken>(potential_tok);
                }
                catch (JsonException je)
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
        }

        public override void SaveState()
        {
            // sets the token's UtcExpirationDate so that when we load it in we can use token.IsFresh to see if we need to refresh.
            DateTime exp_time = DateTime.UtcNow + TimeSpan.FromSeconds(this.token.ExpiresIn);
            this.token.UtcExpirationDate = exp_time;

            string tok_s = JsonConvert.SerializeObject(this.token);
            Console.WriteLine(tok_s);
            
            // store encrypted in the devices secure store.
            SecureStorage.SetAsync("fitbit_tok", tok_s);

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
    }
}
