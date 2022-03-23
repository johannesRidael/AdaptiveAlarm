using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fitbit.Api.Portable;
using Fitbit.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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

            // build a fitbit client and make calls for the required data
            FitbitClient client = new FitbitClient(cred, t);

            //Fitbit.Api.Portable.Models.SleepLogDateBase sleep = client.GetSleepDateAsync(new DateTime(2022, 03, 15)).Result;
            //int occurance = 0;
            //Console.WriteLine($"Sleep Data Summary: Sleep event {occurance} is main sleep: {sleep.Sleep[occurance].IsMainSleep}, duration {sleep.Sleep[occurance].Duration} milliseconds, efficiency score {sleep.Sleep[occurance].Efficiency}"); // this sleep.Sleep object is a list of sleep events (times the user slept) find the main sleep event and use its attributes to read what you want. Includes all sleep stages and wake events in that sleep period.
            //string sleepJson = JsonSerializer.Serialize(sleep);
            //Console.WriteLine(sleepJson);

            //Fitbit.Models.HeartActivitiesIntraday heart = client.GetHeartRateIntraday(new DateTime(2022, 03, 15), HeartRateResolution.fifteenMinute).Result;
            //Console.WriteLine($"Heart Rate Data Summary: {heart.DatasetType}, resting heart rate {heart.ActivitiesHeart.Value}, heart rate {heart.Dataset[0].Value} at time {heart.Dataset[0].Time}");
            //string heartJson = JsonSerializer.Serialize(heart);
            //Console.WriteLine(heartJson);


            /*
             * TODO: Add code to:
             * 1) Read from a local /data/ directory and determine what days need to be downloaded
             * 2) Make calls to the Fitbit API to download those days
             * 3) Serialize the data back to JSON and output it to the /data/ directory mentioned above
             */

            string[] filePaths = Directory.GetFiles(@"..\..\..\data\", "*.json", SearchOption.TopDirectoryOnly);
            HashSet<DateTime> daysPresent = new HashSet<DateTime>();
            foreach (string p in filePaths)
            {
                string[] pTokens = p.Split(separator: '\\');
                string fileName = pTokens[pTokens.Length - 1].Split('.')[0];
                //Console.WriteLine($"{p}: {fileName}");
                if (DateTime.TryParse(fileName, out DateTime date))
                {
                    daysPresent.Add(date);
                    Console.WriteLine($"Found date json file: {fileName}");
                }
                else // not a date
                {
                    Console.WriteLine($"Found non-date json file in data folder: {fileName}");
                }
            }

            int days = (DateTime.Today - DateTime.Parse("2022-02-20")).Days;
            HashSet<DateTime> daysAbsent = new HashSet<DateTime>();
            for (int i = 0; i < days; i++)
            {
                DateTime day = DateTime.Parse("2022-02-20").AddDays(i);
                if (!daysPresent.Contains(day))
                {
                    daysAbsent.Add(day);
                    Console.WriteLine($"absent day found: {day.ToString("yyyy-MM-dd")}");
                }
            }

            foreach (DateTime day in daysAbsent)
            {

                Console.WriteLine($"retreiving and saving data for {day.ToString("yyyy-MM-dd")}");
                // sleep data
                Fitbit.Api.Portable.Models.SleepLogDateBase daysSleep = client.GetSleepDateAsync(day).Result;
                string daysSleepJson = JsonSerializer.Serialize(daysSleep);
                //Console.WriteLine(daysSleepJson);
                File.WriteAllTextAsync($"..\\..\\..\\data\\{day.ToString("yyyy-MM-dd")}.sleep.json", daysSleepJson);

                //// heartrate data
                Fitbit.Models.HeartActivitiesIntraday daysHeart = client.GetHeartRateIntraday(day, HeartRateResolution.fifteenMinute).Result;
                string daysHeartJson = JsonSerializer.Serialize(daysHeart);
                //Console.WriteLine(daysHeartJson);
                File.WriteAllTextAsync($"..\\..\\..\\data\\{day.ToString("yyyy-MM-dd")}.heart.json", daysHeartJson);
            }

            Console.WriteLine("\r\n\r\nData collection completed. Press Enter to Exit.");

            Console.ReadLine();
        }
    }
}
