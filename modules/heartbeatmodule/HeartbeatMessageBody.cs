namespace iot.edge.heartbeat
{
    using System;

    public class HeartbeatMessageBody
    {
        public string deviceId {get; set;}

        public int counter {get; set;}

        public DateTime timeStamp { get; set; }
    }
}