# iot-edge-heartbeat

Azure IoTEdge GA Version module which generates a constant heartbeat.

## Introduction

This is a C# .Net Standard module, written for Azure IoTEdge version GA.

This module can be used to generate a constant heartbeat coming from the IoT Edge module or IoT Edge devices which host this module.

## Docker Hub

A version generated for Docker Linux can be found at https://hub.docker.com/r/svelde/iot-edge-heartbeat/

You can pull it with **docker pull svelde/iot-edge-heartbeat** but I suggest to use **svelde/iot-edge-heartbeat:1.0** when you deploy it using the Azure portal.

## Module Twin

This module supports one 'desired' property in the module twin:

- "interval":5000

This alters the interval of the heartbeat in milliseconds. The default value is 5000.

After reading this value, the module will report the value back.

## Routing input and outputs

This module has no logic attached to the routing input.

The messages created are sent using output **output1**

## Output messages

The output message uses this format:

    ```csharp
    private class HeartbeatMessageBody
    {
        public string deviceIs {get; set;}
        public int counter {get; set;}
        public DateTime timeStamp { get; set; }
    }
    ```

the message is sent with the following message property:

- "content-type", "application/edge-heartbeat-json"

## Code example

This module was based on an Public Preview example shown [here](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-csharp-module)

With the introduction of General Available, some breaking changes are introduced. A new ModuleClient is introduced to replace the DeviceClient.

I have updated the code. Due to some changes in the project structure my project is a bit behind.

## Contribute

This logic is licenced under the MIT license.

Want to contribute? Throw me a pull request....

Want to know more about me? Check out my [blog](http://blog.vandevelde-online.com)
