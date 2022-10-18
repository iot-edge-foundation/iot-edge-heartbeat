namespace iot.edge.heartbeat
{
    using System;
    using System.Collections.Generic;
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

        private static string _moduleId;

        private static string _deviceId;

        private static string _messageType;

        private static UInt16 _counter = 0;

        private static ModuleOutputList _moduleOutputs;

        private static DateTime _lastMessageSent = DateTime.MinValue;

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
            _deviceId = System.Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
            _moduleId = Environment.GetEnvironmentVariable("IOTEDGE_MODULEID");

            Console.WriteLine("      _                         ___      _____   ___     _");
            Console.WriteLine("     /_\\   ___ _  _  _ _  ___  |_ _| ___|_   _| | __| __| | __ _  ___  ");
            Console.WriteLine("    / _ \\ |_ /| || || '_|/ -_)  | | / _ \\ | |   | _| / _` |/ _` |/ -_)");
            Console.WriteLine("   /_/ \\_\\/__| \\_,_||_|  \\___| |___|\\___/ |_|   |___|\\__,_|\\__, |\\___|");
            Console.WriteLine("                                                           |___/");

            Console.WriteLine("    _  _              _   _              _    ");
            Console.WriteLine("   | || |___ __ _ _ _| |_| |__  ___ __ _| |_  ");
            Console.WriteLine("   | __ / -_) _` | '_|  _| '_ \\/ -_) _` |  _| ");
            Console.WriteLine("   |_||_\\___\\__,_|_|  \\__|_.__/\\___\\__,_|\\__| ");

            Console.WriteLine(" ");
            Console.WriteLine("   Copyright © 2019 - IoT Edge Foundation");
            Console.WriteLine(" ");

            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            var ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);

            AddOutputs(ioTHubModuleClient);

            _moduleOutputs.WriteOutputInfo();

            // Attach callback for Twin desired properties updates
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertiesUpdate, ioTHubModuleClient);

            // Execute callback method for Twin desired properties updates
            var twin = await ioTHubModuleClient.GetTwinAsync();
            await onDesiredPropertiesUpdate(twin.Properties.Desired, ioTHubModuleClient);

            await ioTHubModuleClient.OpenAsync();

            Console.WriteLine($"Module '{_deviceId}'-'{_moduleId}' initialized.");

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "getCount",
                getCountMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: getCount");   
            Console.WriteLine("Attached output: output1");
            Console.WriteLine("Supported desired properties: 'interval'.");
            Console.WriteLine("Supported desired properties: 'messageType'.");

            var thread = new Thread(() => ThreadBody(ioTHubModuleClient));
            thread.Start();
        }

        private static void AddOutputs(ModuleClient ioTHubModuleClient)
        {
            _moduleOutputs = new ModuleOutputList();

            var addedOutput1 = _moduleOutputs.Add(new ModuleOutput("output1", ioTHubModuleClient, "heartbeat"));
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

                var heartbeatMessageBody =  new HeartbeatMessageBody();

                if (string.IsNullOrEmpty(_messageType))
                {    
                    heartbeatMessageBody = new HeartbeatMessageBody
                    {
                        deviceId = _deviceId,
                        counter = _counter,
                        timeStamp = DateTime.UtcNow
                    };
                }
                else
                {
                    heartbeatMessageBody = new HeartbeatMessageBodyExtended
                    {
                        deviceId = _deviceId,
                        counter = _counter,
                        timeStamp = DateTime.UtcNow,
                        messageType = _messageType
                    };
                }

                await _moduleOutputs.GetModuleOutput("output1")?.SendMessage(heartbeatMessageBody);

                _lastMessageSent = DateTime.UtcNow;

                Console.WriteLine($"Heartbeat {heartbeatMessageBody.counter} sent at {heartbeatMessageBody.timeStamp}; Next message scheduled for {_lastMessageSent.AddMilliseconds(Interval)}");

                while (DateTime.UtcNow < _lastMessageSent.AddMilliseconds(Interval))
                {
                    Thread.Sleep(500);                    
                }
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

                if (desiredProperties.Contains("messageType")) 
                {
                    if (desiredProperties["messageType"] != null)
                    {
                        _messageType = desiredProperties["messageType"];
                    }
                    else
                    {
                        _messageType = null;
                    }

                    Console.WriteLine($"messageType changed to {_messageType}");

                    reportedProperties["messageType"] = Interval;
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

        static async Task<MethodResponse> getCountMethodCallBack(MethodRequest methodRequest, object userContext)        
        {
            var getCountResponse = new GetCountResponse{ responseState = 0 };

            try
            {
                getCountResponse.count = _counter;                   
            }
            catch (Exception ex)
            {
               getCountResponse.errorMessage = ex.Message;   
               getCountResponse.responseState = -999;
            }            

            var json = JsonConvert.SerializeObject(getCountResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            await Task.Delay(TimeSpan.FromSeconds(0));

            return response;
        }
    }
}