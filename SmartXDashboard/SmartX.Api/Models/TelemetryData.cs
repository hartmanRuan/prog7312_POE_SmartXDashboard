using System;

namespace SmartX.Api.Models
{
    public class TelemetryPacket<T>
    {
        public string PacketId { get; set; } = Guid.NewGuid().ToString("N");
        public string MacAddress { get; set; } = string.Empty;
        public ZoneLocation LocationZone { get; set; } 
        public SensorCategory Category { get; set; }
        public T PayloadValue { get; set; }
        public string MetricUnit { get; set; } = string.Empty;
        public NodeStatus SeverityStatus { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}