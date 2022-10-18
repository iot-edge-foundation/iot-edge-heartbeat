namespace iot.edge.heartbeat
{
    using System;

    public class HeartbeatMessageBodyExtended: HeartbeatMessageBody
    {
        public string messagetype { get; set; }
    }
}