using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SmartXDashboard
{
    public partial class TelemetryStreamView : UserControl
    {
        private List<TelemetrySample> allSamples = new List<TelemetrySample>();

        public TelemetryStreamView()
        {
            InitializeComponent();
            SeedInitialData();
            TelemetryGrid.ItemsSource = allSamples;
        }

        private void SeedInitialData()
        {
            allSamples = new List<TelemetrySample>
            {
                new TelemetrySample { Timestamp = DateTime.Now.AddSeconds(-20).ToString("HH:mm:ss.fff"), MacAddress = "00:1A:2B:3C:4D:5E", Zone = "Zone A", PayloadValue = "24.5", MetricType = "°C", Status = "Normal" },
                new TelemetrySample { Timestamp = DateTime.Now.AddSeconds(-15).ToString("HH:mm:ss.fff"), MacAddress = "00:1A:2B:3C:4D:5F", Zone = "Zone B", PayloadValue = "415.2", MetricType = "Volts", Status = "Warning" },
                new TelemetrySample { Timestamp = DateTime.Now.AddSeconds(-10).ToString("HH:mm:ss.fff"), MacAddress = "00:1A:2B:3C:4D:60", Zone = "Zone C", PayloadValue = "1200", MetricType = "RPM", Status = "Normal" },
                new TelemetrySample { Timestamp = DateTime.Now.AddSeconds(-5).ToString("HH:mm:ss.fff"), MacAddress = "00:1A:2B:3C:4D:5E", Zone = "Zone A", PayloadValue = "88.9", MetricType = "°C", Status = "Critical" }
            };
        }

        private void SimulatePacket_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            TelemetrySample sample = new TelemetrySample
            {
                Timestamp = DateTime.Now.ToString("HH:mm:ss.fff"),
                MacAddress = "00:1A:2B:3C:4D:5E",
                Zone = "Zone A",
                PayloadValue = (rand.Next(200, 900) / 10.0).ToString("F1"),
                MetricType = "°C",
                Status = rand.Next(0, 2) == 0 ? "Normal" : "Warning"
            };

            allSamples.Insert(0, sample);
            ApplyFilters();
        }

        private void SearchMacInput_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void ApplyFilters()
        {
            if (TelemetryGrid == null) return;

            string filterText = SearchMacInput?.Text.Trim().ToLower() ?? "";
            string selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All Streams";

            var filtered = allSamples.Where(s =>
                (string.IsNullOrEmpty(filterText) || s.MacAddress.ToLower().Contains(filterText)) &&
                (selectedStatus == "All Streams" || s.Status.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            TelemetryGrid.ItemsSource = filtered;
        }
    }

    public class TelemetrySample
    {
        public string Timestamp { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string PayloadValue { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}