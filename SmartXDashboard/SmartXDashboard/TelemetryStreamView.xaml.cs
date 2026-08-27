using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SmartXDashboard.Models;
using SmartXDashboard.Services;

namespace SmartXDashboard
{
    public partial class TelemetryStreamView : UserControl
    {
        private readonly TelemetrySimulator _simulator;
        private readonly ExportService _exportService = new ExportService();

        // Internal master buffer for keeping all received packets
        private readonly ObservableCollection<TelemetryPacket<double>> _allPackets
            = new ObservableCollection<TelemetryPacket<double>>();

        // Public collection bound to the TelemetryGrid
        public ObservableCollection<TelemetryPacket<double>> TelemetryStream { get; set; }
            = new ObservableCollection<TelemetryPacket<double>>();

        public TelemetryStreamView()
        {
            InitializeComponent();

            if (TelemetryGrid != null)
            {
                TelemetryGrid.ItemsSource = TelemetryStream;
            }

            // Initialize simulator (fires every 1.5s)
            _simulator = new TelemetrySimulator(1500);
            _simulator.OnTelemetryReceived += Simulator_OnTelemetryReceived;

            // Manage background timer thread with view lifecycle
            Loaded += (s, e) => _simulator.Start();
            Unloaded += (s, e) => _simulator.Stop();
        }

        private void Simulator_OnTelemetryReceived(TelemetryPacket<double> packet)
        {
            // Marshal thread execution to WPF UI Thread
            Dispatcher.Invoke(() =>
            {
                // Cap master buffer at 100 items for performance
                if (_allPackets.Count >= 100)
                {
                    _allPackets.RemoveAt(_allPackets.Count - 1);
                }

                _allPackets.Insert(0, packet);
                ApplyFilters();
            });
        }

        private void SearchMacInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (TelemetryGrid == null) return;

            string filterText = SearchMacInput?.Text.Trim().ToLower() ?? string.Empty;
            string selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Streams";

            // LINQ filtering on MAC Address and NodeStatus severity
            var filtered = _allPackets.Where(packet =>
            {
                bool matchesMac = string.IsNullOrEmpty(filterText) ||
                                  packet.MacAddress.ToLower().Contains(filterText);

                bool matchesStatus = selectedStatus switch
                {
                    "Active" or "Normal" => packet.SeverityStatus == NodeStatus.Active,
                    "Warning" => packet.SeverityStatus == NodeStatus.Warning,
                    "Critical" => packet.SeverityStatus == NodeStatus.Critical,
                    _ => true
                };

                return matchesMac && matchesStatus;
            }).ToList();

            // Refresh UI stream collection
            TelemetryStream.Clear();
            foreach (var item in filtered)
            {
                TelemetryStream.Add(item);
            }
        }

        private void SimulatePacket_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            var packet = new TelemetryPacket<double>(
                "00:1A:2B:3C:4D:5E",
                ZoneLocation.ZoneA_Environmental,
                Math.Round(rand.Next(200, 900) / 10.0, 1),
                "°C",
                rand.Next(0, 2) == 0 ? NodeStatus.Active : NodeStatus.Warning
            );

            _allPackets.Insert(0, packet);
            ApplyFilters();
        }

        // Export button handler linked to SaveFileDialog
        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"TelemetryExport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                bool success = _exportService.ExportTelemetryToCsv(TelemetryStream, saveFileDialog.FileName);

                if (success)
                {
                    MessageBox.Show("Telemetry stream data successfully exported to CSV!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to export telemetry data to CSV.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}