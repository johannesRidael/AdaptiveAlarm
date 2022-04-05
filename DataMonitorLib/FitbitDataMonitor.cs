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

namespace DataMonitorLib
{
    class FitbitDataMonitor : DataMonitor
    {
        private Fitbit.Api.Portable.OAuth2.OAuth2AccessToken token;
        private static string CLIENTID = "Application Client ID";
        private static string CLIENTSECRET = "Application Client Secret";

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
            Thread browserThread = new Thread(() => { Thread.Sleep(500); System.Diagnostics.Process.Start("explorer.exe", $"\"{uri}\""); });
            browserThread.Start();

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

            string code = inputLine.Split(" ")[1].Split("=")[1];
            Console.WriteLine(code);

            this.token = ah.ExchangeAuthCodeForAccessTokenAsync(code).Result;

            // TODO: write the DataMonitor's token to a protected store incase of crash.
            //string tok_s = JsonConvert.SerializeObject(this.token);

            return false;
        }
    }
}
