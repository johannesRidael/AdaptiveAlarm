using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fitbit.Api.Portable;
using Fitbit.Models;

namespace DataCollection_Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            // setup credentials aand authentication helper
            Console.WriteLine("Starting Test");
            FitbitAppCredentials cred = new FitbitAppCredentials();
            cred.ClientId = "Application ID";
            cred.ClientSecret = "Application Secret";
            Fitbit.Api.Portable.OAuth2.OAuth2Helper ah = new Fitbit.Api.Portable.OAuth2.OAuth2Helper(cred, "http://localhost:4306/aa/data-collection");

            // build authentication url and open it in the default web browser
            String authUrl = ah.GenerateAuthUrl(new string[] { "sleep", "heartrate" }, null);
            Console.WriteLine(authUrl);
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
            Console.WriteLine("Got line: " + inputLine);
            writer.WriteLineAsync("HTTP/1.1 200 OK\r\nHello World!\r\n\r\n").Wait(); //TODO: Find a way to get this to give the user a friendly thank you page.
            tcpClient.Close();

            string code = inputLine.Split(" ")[1].Split("=")[1];
            Console.WriteLine(code);

            Fitbit.Api.Portable.OAuth2.OAuth2AccessToken t = ah.ExchangeAuthCodeForAccessTokenAsync(code).Result;

            
            FitbitClient client = new FitbitClient(cred, t);

            Fitbit.Api.Portable.Models.SleepLogDateBase sleep = client.GetSleepDateAsync(new DateTime(2022, 03, 15)).Result;
            int occurance = 0;
            Console.WriteLine($"Sleep Data Summary: Sleep event {occurance} is main sleep: {sleep.Sleep[occurance].IsMainSleep}, duration {sleep.Sleep[occurance].Duration} milliseconds, efficiency score {sleep.Sleep[occurance].Efficiency}"); // this sleep.Sleep object is a list of sleep events (times the user slept) find the main sleep event and use its attributes to read what you want. Includes all sleep stages and wake events in that sleep period.

            Fitbit.Models.HeartActivitiesIntraday heart = client.GetHeartRateIntraday(new DateTime(2022, 03, 15), HeartRateResolution.fifteenMinute).Result;
            Console.WriteLine($"Heart Rate Data Summary: {heart.DatasetType}, resting heart rate {heart.ActivitiesHeart.Value}, heart rate {heart.Dataset[0].Value} at time {heart.Dataset[0].Time}");
            Console.ReadLine();
        }
    }
}
