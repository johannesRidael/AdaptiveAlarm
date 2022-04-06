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

namespace DataMonitorLib
{
    public class FitbitDataMonitor : DataMonitor
    {
        private Fitbit.Api.Portable.OAuth2.OAuth2AccessToken token;
        // Temporary ClientId and Secret for use in testing.
        private static string CLIENTID = "2388MW";
        private static string CLIENTSECRET = "778c9cc1bd4c44842e876cef351ec899";

        public FitbitDataMonitor(){}

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

        public override void LoadState()
        {
            //TODO: If a prior state was saved locally then load it in.
            throw new NotImplementedException();
        }

        public override void SaveState()
        {
            //TODO: Save the current state locally.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to authenticate the Fitbit DataMonitor and set its Access and Refresh Tokens to new values.
        /// </summary>
        /// <returns> Whether or not authentication was successful. </returns>
        public bool Authenticate()
        {
            FitbitAppCredentials credentials = new FitbitAppCredentials();
            credentials.ClientId = CLIENTID;
            credentials.ClientSecret = CLIENTSECRET;
            Fitbit.Api.Portable.OAuth2.OAuth2Helper ah = new Fitbit.Api.Portable.OAuth2.OAuth2Helper(credentials, "http://localhost:4306/aa/data-collection");

            // build authentication url and open it in the default web browser
            String authUrl = ah.GenerateAuthUrl(new string[] { "sleep", "heartrate" }, null);
            //Console.WriteLine(authUrl);
            Uri uri = new Uri(authUrl);

            // open a browser to authenticate w/ Fitbit.
            //TODO: maybe change this to open a WebView instead (allows greater control and eliminates the need for a TCP listener).
            try
            {
                Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred); // well that was easy!
            }
            catch (Exception ex)
            {
                // An unexpected error occured. No browser may be installed on the device.
            }

            //TODO: ensure that token retrieval is actually happening

            // start a tcp listener to get the user code from the fitbit response
            int port = 4306;
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();

            TcpClient tcpClient = listener.AcceptTcpClient();
            NetworkStream stream = tcpClient.GetStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            StreamReader reader = new StreamReader(stream, Encoding.ASCII);

            string inputLine = reader.ReadLine();
            //Console.WriteLine("Got line: " + inputLine);
            writer.WriteLineAsync("HTTP/1.1 200 OK\r\nHello World!\r\n\r\n").Wait(); //TODO: Find a way to get this to give the user a friendly thank you page.
            tcpClient.Close();

            string code = inputLine.Split(' ')[1].Split('=')[1];
            Console.WriteLine("Auth Code: " + code);

            this.token = ah.ExchangeAuthCodeForAccessTokenAsync(code).Result;
            Console.WriteLine("Token: " + this.token.Token);

            //// TODO: write the DataMonitor's token to a protected store incase of crash.
            ////string tok_s = JsonConvert.SerializeObject(this.token);

            ////Application.Current.Properties

            return false;
        }
    }
}
