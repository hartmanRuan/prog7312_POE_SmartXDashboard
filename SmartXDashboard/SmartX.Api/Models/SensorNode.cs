namespace SmartX.Api.Models
{
    public class SensorNode
    {
        public string MacAddress { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;

        // Accept integers or strings for Enums passed from WPF
        public ZoneLocation LocationZone { get; set; }
        public SensorCategory Category { get; set; }
        public NodeStatus Status { get; set; }

        public DateTime ProvisionedTimestamp { get; set; } = DateTime.Now;

        // Metadata Attachment Properties
        public string? ConfigFileName { get; set; }
        public long ConfigFileSizeKB { get; set; }
        public string? AttachedFilePath { get; set; }
    }
}
