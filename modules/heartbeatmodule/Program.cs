namespace iot.edge.heartbeat
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json; 

    class Program
    {
        private const int DefaultInterval = 5000;

        private static string _deviceId; 

        private static UInt16 _counter = 0;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);

            // Attach callback for Twin desired properties updates
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertiesUpdate, ioTHubModuleClient);

            // Execute callback method for Twin desired properties updates
            var twin = await ioTHubModuleClient.GetTwinAsync();
            await onDesiredPropertiesUpdate(twin.Properties.Desired, ioTHubModuleClient);

            await ioTHubModuleClient.OpenAsync();

            Console.WriteLine(@"");
            Console.WriteLine(@"     /$$$$$$      /$$$$$$  /$$    /$$ /$$$$$$$$ /$$       /$$$$$$$  /$$$$$$$$ ");
            Console.WriteLine(@"   /$$$__  $$$   /$$__  $$| $$   | $$| $$_____/| $$      | $$__  $$| $$_____/ ");
            Console.WriteLine(@"  /$$_/  \_  $$ | $$  \__/| $$   | $$| $$      | $$      | $$  \ $$| $$       ");
            Console.WriteLine(@" /$$/ /$$$$$  $$|  $$$$$$ |  $$ / $$/| $$$$$   | $$      | $$  | $$| $$$$$    ");
            Console.WriteLine(@"| $$ /$$  $$| $$ \____  $$ \  $$ $$/ | $$__/   | $$      | $$  | $$| $$__/    ");
            Console.WriteLine(@"| $$| $$\ $$| $$ /$$  \ $$  \  $$$/  | $$      | $$      | $$  | $$| $$       ");
            Console.WriteLine(@"| $$|  $$$$$$$$/|  $$$$$$/   \  $/   | $$$$$$$$| $$$$$$$$| $$$$$$$/| $$$$$$$$ ");
            Console.WriteLine(@"|  $$\________/  \______/     \_/    |________/|________/|_______/ |________/ ");
            Console.WriteLine(@" \  $$$   /$$$                                                                ");
            Console.WriteLine(@"  \_  $$$$$$_/                                                                ");
            Console.WriteLine(@"    \______/                                                                  ");
            Console.WriteLine("Heartbeat IoT Hub module client initialized.");
            Console.WriteLine("This module uses outputs 'output1'.");

            var thread = new Thread(() => ThreadBody(ioTHubModuleClient));
            thread.Start();

            _deviceId = System.Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
        }

        private static async void ThreadBody(object userContext)
        {
            while (true)
            {
                var client = userContext as ModuleClient;

                if (client == null)
                {
                    throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
                }

                _counter++;

                var heartbeatMessageBody = new HeartbeatMessageBody
                {
                    deviceId = _deviceId,
                    counter = _counter,
                    timeStamp = DateTime.UtcNow,
                };

                var jsonMessage = JsonConvert.SerializeObject(heartbeatMessageBody);

                var pipeMessage = new Message(Encoding.UTF8.GetBytes(jsonMessage));

                // Set message body type and content encoding for routing using decoded body values. 
                pipeMessage.ContentEncoding = "utf-8"; 
                pipeMessage.ContentType = "application/json"; 
                
                // Set a property as a fingerprint for this module
                pipeMessage.Properties.Add("content-type", "application/edge-heartbeat-json");

                await client.SendEventAsync("output1", pipeMessage);

                Console.WriteLine($"Heartbeat {heartbeatMessageBody.counter} sent at {heartbeatMessageBody.timeStamp}");

                Thread.Sleep(Interval);
            }
        }

        private static int Interval { get; set; } = DefaultInterval;

        private static Task onDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            if (desiredProperties.Count == 0)
            {
                return Task.CompletedTask;
            }

            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                var client = userContext as ModuleClient;

                if (client == null)
                {
                    throw new InvalidOperationException($"UserContext doesn't contain expected ModuleClient");
                }

                var reportedProperties = new TwinCollection();

                if (desiredProperties.Contains("interval")) 
                {
                    if (desiredProperties["interval"] != null)
                    {
                        Interval = desiredProperties["interval"];
                    }
                    else
                    {
                        Interval = DefaultInterval;
                    }

                    Console.WriteLine($"Interval changed to {Interval}");

                    reportedProperties["interval"] = Interval;
                }

                if (reportedProperties.Count > 0)
                {
                    client.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
                }
            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
            }

            return Task.CompletedTask;
        }

        private class HeartbeatMessageBody
        {
            public string deviceId {get; set;}

            public int counter {get; set;}

            public DateTime timeStamp { get; set; }
        }
    }
}
