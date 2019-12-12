# iot-edge-heartbeat

Azure IoTEdge GA Version module which generates a constant heartbeat.

## Introduction

This is a C# .Net Standard module, written for Azure IoTEdge version GA.

This module can be used to generate a constant heartbeat coming from the IoT Edge module or IoT Edge devices which host this module.

You can also check for quality of the connection or for missing telemetry using the counter provided.

## Docker Hub

A version generated for Docker Linux can be found at https://hub.docker.com/r/svelde/iot-edge-heartbeat/

You can pull it with **docker pull svelde/iot-edge-heartbeat** but I suggest to use **svelde/iot-edge-heartbeat:2.4.0** when you deploy it using the Azure portal.

## Module Twin

This module supports one 'desired' property in the module twin:

- "interval" : 10000

or 

```
"desired": {
    "interval" : 10000
}
```

This alters the interval of the heartbeat in milliseconds. The default value is 5000.

After reading this desired property, the module will report the value back as reported property:

```
"reported": {
    "interval" : 10000
}
```

## Routing input and outputs

This module has no logic attached to the routing input.

The messages created are sent using output **output1**

## Output messages

The output message uses this format:

```
private class HeartbeatMessageBody
{
    public string deviceId {get; set;}
    public int counter {get; set;}
    public DateTime timeStamp { get; set; }
}
```

the message is sent with the following message property:

- "content-type", "application/edge-heartbeat-json"

## Direct methods

### getCount

With the Direct method 'getCount', you can read the current counter.

As a response you get:

```
public class GetCountResponse 
{
    public int count { get; set; }
    public int responseState { get; set; }
    public string errorMessage { get; set; }
}
```

## Contribute

This logic is licenced under the MIT license.

The IoT Edge Heartbeat module was originally developed by [Sander van de Velde](http://blog.vandevelde-online.com).

This module is now donated to the IoT Edge Foundation.

Want to contribute? Throw a pull request....