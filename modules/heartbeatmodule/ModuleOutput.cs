namespace iot.edge.heartbeat
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    public class ModuleOutput
    {
        private ModuleClient _ioTHubModuleClient;

        public ModuleOutput(string name, ModuleClient ioTHubModuleClient) 
        {
            Initialize(name,  ioTHubModuleClient);
        }

        public ModuleOutput(string name, ModuleClient ioTHubModuleClient, string context) 
        {
            Context = context;

            Initialize(name,  ioTHubModuleClient);
        }

        public string Name { get; private set; } = string.Empty;

        public string Context {get; private set;} = string.Empty;

        public Dictionary<string, string> Properties { get; private set; }


        public async Task SendMessage(object messageBody)
        {
            var jsonMessage = JsonConvert.SerializeObject(messageBody);

            var message = new Message(Encoding.UTF8.GetBytes(jsonMessage));

            // Set message body type and content encoding for routing using decoded body values.
            message.ContentEncoding = "utf-8";
            message.ContentType = "application/json";

            foreach (var p in Properties)
            {
                message.Properties.Add(p);
            }

            await _ioTHubModuleClient.SendEventAsync(Name, message);
        }

        private void Initialize(string name, ModuleClient ioTHubModuleClient) 
        {
            Name = name;

            _ioTHubModuleClient = ioTHubModuleClient;

            CreateProperties();
        }

        private void CreateProperties()
        {
            Properties = new Dictionary<string, string>();

            var contentType = $"application/edge";

            if (Name != string.Empty)
            {
                contentType += $"-{Name}";
            }

            if (Context != string.Empty)
            {
                contentType += $"-{Context}";
            }

            contentType += "-json";

            // Set a property as a fingerprint for this module
            Properties.Add("content-type", contentType);
        }
    }
}