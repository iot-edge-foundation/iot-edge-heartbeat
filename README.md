# iot-edge-heartbeat

Azure IoTEdge GA Version module which generates a constant heartbeat.

## Introduction

This is a C# .Net Azure IoTEdge module.

This module can be used to generate a constant heartbeat coming from the IoT Edge module or IoT Edge devices which host this module.

You can also check for quality of the connection or for missing telemetry using the counter provided.

## Docker Hub

A version generated for Docker Linux can be found at https://hub.docker.com/r/iotedgefoundation/iot-edge-heartbeat/

You can pull it with **docker pull iotedgefoundation/iot-edge-heartbeat** but I suggest to use **iotedgefoundation/iot-edge-heartbeat:3.2.0** when you deploy it using the Azure portal.

## Module Twin

This module supports one 'desired' property in the module twin:

- "interval" : 10000
- "messagetype" : "<MyCompany>:Heartbeat;<version>"

or 

```
"desired": {
    "interval" : 10000,
    "messagetype" : "mycompany:heartbeat;1"
}
```

This alters the interval of the heartbeat in milliseconds. The default value is 5000.

After reading this desired property, the module will report the value back as reported property:

```
"reported": {
    "interval" : 10000,
    "messagetype" : "MyCompany:Heartbeat;1"
}
```

We recommend checking the "Module Twin" for reported configuration to make sure values are correctly propagated.

## Routing input and outputs

This module has no logic attached to the routing input.

The messages created are sent using output **output1**

```
FROM /messages/modules/heartbeat/outputs/output1 into $upstream
```

## Output messages

The output message uses this format:

```
private class HeartbeatMessageBody
{
    public string deviceId {get; set;}
    public int counter {get; set;}
    public DateTime timeStamp { get; set; }
    public string messagetype { get; set; }   // only when desired property 'messagetype' is filled 
}
```

If provided, the 'messagetype' needs to support a format like 'mycompany:heartbeat;1'; 

The message is sent with the following application message property:

- "content-type", "application/edge-heartbeat-json"

## Direct methods

### getCount

With the Direct method 'getCount', you can read the current counter.

As a response you get:

```
public class GetCountResponse 
{
    public int count { get; set; }
    public string messageType {get; set;}
    public int responseState { get; set; }
    public string errorMessage { get; set; }
}
```

## Contribute

This logic is licenced under the MIT license.

The IoT Edge Heartbeat module was originally developed by [Sander van de Velde](http://blog.vandevelde-online.com).

This module is now donated to the [IoT Edge Foundation](https://github.com/iot-edge-foundation/iot-edge-heartbeat).

Want to contribute? Throw a pull request....
