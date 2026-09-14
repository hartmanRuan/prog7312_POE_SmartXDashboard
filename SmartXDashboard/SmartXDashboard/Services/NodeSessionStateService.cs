namespace SmartXDashboard.Services
{
    public class NodeSessionStateService
    {
        private static readonly NodeSessionStateService _instance = new();
        public static NodeSessionStateService Instance => _instance;

        public int CurrentUserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsNodeRegistered { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public string BarcodeValue { get; set; } = string.Empty;
        public string LocationZone { get; set; } = string.Empty;
    }
}