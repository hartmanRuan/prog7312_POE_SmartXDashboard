using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartX.Api.Models;
using SmartXDashboard.Models;
using SmartXDashboard.Services;
using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, bool> _nodeInSpikeState = new();

        public ObservableCollection<SpikeRecord> SpikeHistory { get; set; } = new();

        public ISeries[] Series { get; set; }
        private string _selectedMacFilter = "ALL";
        private bool _isNodeRegistered = false;

        public TelemetryStreamView()
        {
            InitializeComponent();

            var lineSeries = new LineSeries<LiveChartsCore.Defaults.ObservableValue>
            {
                Values = _chartValues,
                Fill = null,
                LineSmoothness = 0,
                GeometrySize = 10
            };

            lineSeries.PointMeasured += point =>
            {
                if (point.Context.Visual is null) return;

                if (point.Coordinate.PrimaryValue > UpperThreshold || point.Coordinate.PrimaryValue < LowerThreshold)
                {
                    point.Context.Visual.Fill = new SolidColorPaint(SKColors.Red);
                }
                else
                {
                    point.Context.Visual.Fill = new SolidColorPaint(SKColors.Blue);
                }
            };

            Series = new ISeries[] { lineSeries };
            DataContext = this;
        }

        private async void TelemetryStreamView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await PopulateNodeFilterDropdownAsync();

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
                        MetricUnit = "°C", // Locked strictly to Degree Celsius
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

                    // Enforce 1-node registration limit per user session
                    if (registeredNodes.Count >= 1)
                    {
                        _isNodeRegistered = true;
                        // Lock registration UI or flag here if a registration button exists
                    }

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
                    _selectedMacFilter = null;
                }
                else
                {
                    _selectedMacFilter = selectedItem.Tag?.ToString() ?? content;
                }
            }
            else if (NodeFilterComboBox.SelectedItem is string mac)
            {
                _selectedMacFilter = mac == "All Registered Nodes" ? null : mac;
            }

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
                Name = "Temperature (°C)",
                LabelsPaint = new SolidColorPaint(SkiaSharp.SKColors.Black),
                NamePaint = new SolidColorPaint(SkiaSharp.SKColors.Black),
                TextSize = 12
            }
        };

        private const double UpperThreshold = 85.0;
        private const double LowerThreshold = 10.0;

        private void ProcessIncomingPacket(SmartXDashboard.Models.TelemetryPacket<double> packet)
        {
            bool isViolation = packet.PayloadValue > UpperThreshold || packet.PayloadValue < LowerThreshold;

            if (!_nodeInSpikeState.ContainsKey(packet.MacAddress))
            {
                _nodeInSpikeState[packet.MacAddress] = false;
            }

            if (isViolation)
            {
                if (!_nodeInSpikeState[packet.MacAddress])
                {
                    _nodeInSpikeState[packet.MacAddress] = true;
                    Dispatcher.Invoke(() =>
                    {
                        SpikeHistory.Insert(0, new SpikeRecord
                        {
                            Timestamp = packet.Timestamp,
                            MacAddress = packet.MacAddress,
                            Value = packet.PayloadValue,
                            Unit = "°C"
                        });
                    });
                }
            }
            else
            {
                _nodeInSpikeState[packet.MacAddress] = false;
            }
        }
    }

    public class SpikeRecord
    {
        public DateTime Timestamp { get; set; }
        public string MacAddress { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }
}