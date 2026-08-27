using System;

namespace SmartXDashboard.Models
{
    public class SensorNode
    {
        public string MacAddress { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;
        public ZoneLocation LocationZone { get; set; }
        public SensorCategory Category { get; set; }
        public NodeStatus Status { get; set; } = NodeStatus.Unprovisioned;
        public string ConfigFileName { get; set; } = string.Empty;
        public long ConfigFileSizeKB { get; set; }
        public DateTime ProvisionedTimestamp { get; set; } = DateTime.Now;

        public SensorNode() { }

        public SensorNode(string macAddress, ZoneLocation zone, SensorCategory category, string configFileName = "", long configFileSizeKB = 0)
        {
            MacAddress = macAddress;
            NodeId = $"NODE-{macAddress.Replace(":", "").ToUpper()}";
            LocationZone = zone;
            Category = category;
            Status = NodeStatus.Active;
            ConfigFileName = configFileName;
            ConfigFileSizeKB = configFileSizeKB;
            ProvisionedTimestamp = DateTime.Now;
        }
    }
}