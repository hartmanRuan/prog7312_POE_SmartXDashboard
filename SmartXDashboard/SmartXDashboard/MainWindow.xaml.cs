using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SmartXDashboard.Models;
using SmartXDashboard.Services;

namespace SmartXDashboard
{
    public partial class MainWindow : Window
    {
        private readonly AnalyticsService _analyticsService = new();
        private readonly TelemetrySimulator _simulator = new();

        public MainWindow()
        {
            InitializeComponent();

            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new SensorIngestionView());

            RefreshDashboardMetrics();
        }

        private void NavProvisioning_Click(object sender, RoutedEventArgs e)
        {
            _simulator.Stop(); // Stop simulation when leaving telemetry view
            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new SensorIngestionView());
        }

        private void NavTelemetry_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new TelemetryStreamView());
            _simulator.Start();
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            _simulator.Stop();
            // Clear session state on sign out
            NodeSessionStateService.Instance.CurrentUserId = 0;
            NodeSessionStateService.Instance.Username = string.Empty;
            NodeSessionStateService.Instance.IsNodeRegistered = false;
            NodeSessionStateService.Instance.MacAddress = string.Empty;
            NodeSessionStateService.Instance.BarcodeValue = string.Empty;
            NodeSessionStateService.Instance.LocationZone = string.Empty;

            LoginWindow login = new();
            login.Show();
            this.Close();
        }

        public void RefreshDashboardMetrics(IEnumerable<TelemetryPacket<double>> livePackets = null)
        {
            var samplePackets = livePackets ?? GetInitialSamplePackets();
            var metrics = _analyticsService.CalculateMetrics(samplePackets);

            if (FindName("TotalNodesText") is TextBlock totalNodesText)
                totalNodesText.Text = metrics.ActiveNodesCount.ToString();

            if (FindName("TotalPacketsText") is TextBlock totalPacketsText)
                totalPacketsText.Text = metrics.TotalPacketsReceived.ToString();

            if (FindName("AvgValueText") is TextBlock avgValueText)
                avgValueText.Text = $"{metrics.AveragePayloadValue:F1}";

            if (FindName("WarningAlertsText") is TextBlock warningAlertsText)
                warningAlertsText.Text = metrics.WarningAlertsCount.ToString();
        }

        private List<TelemetryPacket<double>> GetInitialSamplePackets()
        {
            var list = new List<TelemetryPacket<double>>();

            // Fallback sample packet bounded to current user session MAC if available
            string mac = NodeSessionStateService.Instance.MacAddress;
            if (!string.IsNullOrEmpty(mac))
            {
                list.Add(new TelemetryPacket<double>
                {
                    MacAddress = mac,
                    PayloadValue = 22.4,
                    MetricUnit = "°C",
                    Timestamp = System.DateTime.Now
                });
            }

            return list;
        }
    }
}