using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SmartXDashboard.Models;
using SmartXDashboard.Services;

namespace SmartXDashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AnalyticsService _analyticsService = new AnalyticsService();

        public MainWindow()
        {
            InitializeComponent();

            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new SensorIngestionView());

            RefreshDashboardMetrics();
        }

        private void NavProvisioning_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new SensorIngestionView());
        }

        private void NavTelemetry_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new TelemetryStreamView());
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        /// <summary>
        /// Recalculates metrics using AnalyticsService and updates header/summary card controls.
        /// </summary>
        public void RefreshDashboardMetrics(IEnumerable<TelemetryPacket<double>> livePackets = null)
        {
            var samplePackets = livePackets ?? GetInitialSamplePackets();
            var metrics = _analyticsService.CalculateMetrics(samplePackets);

            // Safely assign metrics if header or summary text controls exist in MainWindow.xaml
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
            var nodes = SensorRepository.Instance.GetAllNodes().ToList();
            var list = new List<TelemetryPacket<double>>();

            foreach (var node in nodes)
            {
                list.Add(new TelemetryPacket<double>(node.MacAddress, node.LocationZone, 22.4, "°C", NodeStatus.Active));
            }

            return list;
        }
    }
}