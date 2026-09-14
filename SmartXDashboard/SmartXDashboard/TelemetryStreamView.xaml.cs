using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SmartXDashboard.Models;
using SmartXDashboard.Services;

namespace SmartXDashboard
{
    public partial class TelemetryStreamView : UserControl
    {
        private readonly TelemetryApiClient _apiClient = new();
        private readonly DispatcherTimer _pollTimer = new();
        private readonly ObservableCollection<TelemetryPacket<double>> _telemetryLog = new();
        private readonly ObservableCollection<double> _chartValues = new();
        private string _selectedMacFilter = "ALL";

        public TelemetryStreamView()
        {
            InitializeComponent();

            // 1. Hook up the DataGrid to your telemetry log collection
            if (TelemetryDataGrid != null)
            {
                TelemetryDataGrid.ItemsSource = _telemetryLog;
            }
        }

        private async void TelemetryStreamView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await PopulateNodeFilterDropdownAsync();

                // 1-second polling loop
                _pollTimer.Interval = TimeSpan.FromSeconds(1);
                _pollTimer.Tick += async (s, args) => await FetchLatestTelemetryAsync();
                _pollTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Telemetry View Failed to Load: {ex.Message}\n\n{ex.StackTrace}",
                                "UI Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TelemetryStreamView_Unloaded(object sender, RoutedEventArgs e)
        {
            _pollTimer.Stop();
        }

        private async Task FetchLatestTelemetryAsync()
        {
            try
            {
                var readings = await _apiClient.GetTelemetryAsync(_selectedMacFilter);

                if (readings == null || !readings.Any()) return;

                if (EmptyChartPrompt != null && EmptyChartPrompt.Visibility == Visibility.Visible)
                {
                    EmptyChartPrompt.Visibility = Visibility.Collapsed;
                }

                _telemetryLog.Clear();
                _chartValues.Clear();

                foreach (var packet in readings.TakeLast(20))
                {
                    _telemetryLog.Add(packet);
                    _chartValues.Add(packet.PayloadValue);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Telemetry Fetch Error: {ex.Message}");
            }
        }

        private bool _isPopulatingDropdown = false;

        private async Task PopulateNodeFilterDropdownAsync()
        {
            _isPopulatingDropdown = true;

            NodeFilterComboBox.Items.Clear();

            ComboBoxItem allItem = new ComboBoxItem
            {
                Content = "All Registered Nodes",
                Tag = "ALL",
                Foreground = System.Windows.Media.Brushes.Black,
                IsSelected = true
            };
            NodeFilterComboBox.Items.Add(allItem);

            try
            {
                var registeredNodes = await _apiClient.GetNodesAsync();
                if (registeredNodes != null)
                {
                    ActiveNodeStatusText.Text = $"Active Nodes: {registeredNodes.Count}";

                    foreach (var node in registeredNodes)
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = $"{node.MacAddress} ({node.LocationZone})",
                            Tag = node.MacAddress,
                            Foreground = System.Windows.Media.Brushes.Black
                        };
                        NodeFilterComboBox.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error populating nodes: {ex.Message}");
            }
            finally
            {
                _isPopulatingDropdown = false;
            }
        }

        private void NodeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isPopulatingDropdown) return; // Prevent premature clearing

            if (NodeFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _selectedMacFilter = selectedItem.Tag?.ToString() ?? "ALL";

                if (ChartSelectedNodeLabel != null)
                {
                    ChartSelectedNodeLabel.Text = _selectedMacFilter == "ALL"
                        ? "Showing: All Registered Streams"
                        : $"Showing Filtered: {_selectedMacFilter}";
                }

                _telemetryLog.Clear();
                _chartValues.Clear();
            }
        }
    }
}