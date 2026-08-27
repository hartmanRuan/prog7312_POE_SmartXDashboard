using System;

namespace SmartXDashboard.Models
{
    public class TelemetryPacket<T>
    {
        public string PacketId { get; set; } = Guid.NewGuid().ToString("N");
        public string MacAddress { get; set; } = string.Empty;
        public ZoneLocation LocationZone { get; set; }
        public T PayloadValue { get; set; }
        public string MetricUnit { get; set; } = string.Empty;
        public NodeStatus SeverityStatus { get; set; } = NodeStatus.Active;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public TelemetryPacket() { }

        public TelemetryPacket(string macAddress, ZoneLocation zone, T payloadValue, string metricUnit, NodeStatus status)
        {
            MacAddress = macAddress;
            LocationZone = zone;
            PayloadValue = payloadValue;
            MetricUnit = metricUnit;
            SeverityStatus = status;
            Timestamp = DateTime.Now;
        }

        public string GetFormattedTimestamp()
        {
            return Timestamp.ToString("HH:mm:ss.fff");
        }
    }
}