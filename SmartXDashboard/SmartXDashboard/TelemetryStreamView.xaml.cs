using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartX.Api.Models;
using SmartXDashboard.Models;
using SmartXDashboard.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SmartXDashboard
{
    public partial class TelemetryStreamView : UserControl
    {
        private readonly TelemetryApiClient _apiClient = new();
        private readonly DispatcherTimer _pollTimer = new();
        private readonly ObservableCollection<SmartX.Api.Models.TelemetryPacket<double>> _telemetryLog = new();
        private readonly ObservableCollection<LiveChartsCore.Defaults.ObservableValue> _chartValues = new();

        public ISeries[] Series { get; set; }
        private string _selectedMacFilter = "ALL";

        

        public TelemetryStreamView()
        {
            InitializeComponent();
            Series = new ISeries[]
{
    new LineSeries<LiveChartsCore.Defaults.ObservableValue>
    {
        Values = _chartValues,
        Fill = null,
        LineSmoothness = 0
    }
};
            DataContext = this;
            // 1. Hook up the DataGrid to your telemetry log collection
            /*if (TelemetryDataGrid != null)
            {
                TelemetryDataGrid.ItemsSource = _telemetryLog;
            }*/
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

        private bool _isFetching = false;

        private async Task FetchLatestTelemetryAsync()
        {
            if (_isFetching) return;
            _isFetching = true;

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
                    var localPacket = new SmartXDashboard.Models.TelemetryPacket<double>
                    {
                        Timestamp = packet.Timestamp,
                        MacAddress = packet.MacAddress,
                        LocationZone = (SmartXDashboard.Models.ZoneLocation)packet.LocationZone,
                        PayloadValue = Convert.ToDouble(packet.PayloadValue),
                        MetricUnit = packet.MetricUnit,
                        SeverityStatus = packet.SeverityStatus
                    };

                    _chartValues.Add(new LiveChartsCore.Defaults.ObservableValue(localPacket.PayloadValue));
                    ProcessIncomingPacket(localPacket);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                _isFetching = false;
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
                var registeredNodes = await _apiClient.GetActiveNodesAsync();
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
            if (NodeFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content?.ToString();

                if (content == "All Registered Nodes" || string.IsNullOrEmpty(content))
                {
                    _selectedMacFilter = null; // Pass null so API fetches all nodes
                }
                else
                {
                    // If you store the MAC address in the Tag property or use the content string
                    _selectedMacFilter = selectedItem.Tag?.ToString() ?? content;
                }
            }
            else if (NodeFilterComboBox.SelectedItem is string mac)
            {
                _selectedMacFilter = mac == "All Registered Nodes" ? null : mac;
            }

            // Optional: Trigger an immediate fetch so you don't wait for the next timer tick
            _ = FetchLatestTelemetryAsync();
        }
        public Axis[] XAxes { get; set; } = new Axis[]
{
    new Axis
    {
        Name = "Time / Index",
        LabelsPaint = new SolidColorPaint(SkiaSharp.SKColors.Black),
        NamePaint = new SolidColorPaint(SkiaSharp.SKColors.Black),
        TextSize = 12,
        MinStep = 1
    }
};

        public Axis[] YAxes { get; set; } = new Axis[]
        {
    new Axis
    {
        Name = "Payload Value",
        LabelsPaint = new SolidColorPaint(SkiaSharp.SKColors.Black),
        NamePaint = new SolidColorPaint(SkiaSharp.SKColors.Black),
        TextSize = 12
    }
        };



        private const double UpperThreshold = 85.0;
        private const double LowerThreshold = 10.0;

        private void ProcessIncomingPacket(SmartXDashboard.Models.TelemetryPacket<double> packet)
        {
            if (packet.PayloadValue > UpperThreshold || packet.PayloadValue < LowerThreshold)
            {
                Dispatcher.Invoke(() =>
                {
                    WarningBanner.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3A1E1E"));
                    WarningText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5252"));
                    WarningText.Text = $"WARNING: Node [{packet.MacAddress}] out of range! Value: {packet.PayloadValue:F2} {packet.MetricUnit}";
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    WarningBanner.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1E3A1E"));
                    WarningText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4CAF50"));
                    WarningText.Text = "STATUS: NORMAL - All telemetry streams within safe operating thresholds";
                });
            }
        }

        private void TriggerWarning(string message)
        {
            // Execute on UI thread if called from background thread
            Dispatcher.Invoke(() =>
            {
                WarningBanner.Background = System.Windows.Media.Brushes.Red;
                WarningText.Text = message;
            });
        }

        private void ClearWarning()
        {
            Dispatcher.Invoke(() =>
            {
                WarningBanner.Background = System.Windows.Media.Brushes.ForestGreen;
                WarningText.Text = "Status: Normal";
            });
        }




    }


}