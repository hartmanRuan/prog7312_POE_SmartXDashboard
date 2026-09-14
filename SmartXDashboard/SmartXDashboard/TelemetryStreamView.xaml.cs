using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
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
        private readonly ObservableCollection<SmartXDashboard.Models.TelemetryPacket<double>> _telemetryLog = new();
        private readonly ObservableCollection<LiveChartsCore.Defaults.ObservableValue> _chartValues = new();
        private readonly Dictionary<string, bool> _nodeInSpikeState = new();

        public ObservableCollection<SpikeRecord> SpikeHistory { get; set; } = new();

        public ISeries[] Series { get; set; }
        private string _selectedMacFilter = string.Empty;

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

            // --- HARDCODED 20 VALUES FOR CHART VISUALIZATION ---
            var hardcodedReadings = new double[]
            {
                22.5, 24.1, 26.8, 25.0, 23.4,
                27.2, 29.5, 31.0, 88.5, 24.0, // 88.5 triggers first red spike
                22.1, 23.9, 25.4, 26.1, 24.8,
                29.0, 30.2, 92.1, 25.6, 23.2  // 92.1 triggers second red spike
            };

            foreach (var val in hardcodedReadings)
            {
                _chartValues.Add(new LiveChartsCore.Defaults.ObservableValue(val));
            }

            // --- HARDCODED SPIKE RECORDS FOR THE TABLE ---
            SpikeHistory.Add(new SpikeRecord
            {
                Timestamp = DateTime.Now.AddMinutes(-10),
                MacAddress = "00:1A:2B:3C:4D:5E",
                Value = 88.5,
                Unit = "°C"
            });

            SpikeHistory.Add(new SpikeRecord
            {
                Timestamp = DateTime.Now.AddMinutes(-2),
                MacAddress = "00:1A:2B:3C:4D:5E",
                Value = 92.1,
                Unit = "°C"
            });
        }

        private async void TelemetryStreamView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _selectedMacFilter = NodeSessionStateService.Instance.MacAddress;

                if (NodeFilterComboBox != null)
                {
                    NodeFilterComboBox.Visibility = Visibility.Collapsed;
                }

                if (ActiveNodeStatusText != null)
                {
                    ActiveNodeStatusText.Text = string.IsNullOrEmpty(_selectedMacFilter)
                        ? "Active Node: None Registered"
                        : $"Active Node: {_selectedMacFilter} ({NodeSessionStateService.Instance.LocationZone})";
                }

                _pollTimer.Interval = TimeSpan.FromSeconds(1);
                _pollTimer.Tick += async (s, args) => await FetchLatestTelemetryAsync();
                _pollTimer.Start();

                await FetchLatestTelemetryAsync();
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
                string targetMac = string.IsNullOrEmpty(NodeSessionStateService.Instance.MacAddress)
                    ? _selectedMacFilter
                    : NodeSessionStateService.Instance.MacAddress;

                if (string.IsNullOrEmpty(targetMac))
                {
                    targetMac = "00:1A:2B:3C:4D:5E";
                }

                var readings = await _apiClient.GetTelemetryAsync(targetMac);
                if (readings == null || !readings.Any()) return;

                if (EmptyChartPrompt != null && EmptyChartPrompt.Visibility == Visibility.Visible)
                {
                    EmptyChartPrompt.Visibility = Visibility.Collapsed;
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

        private void NodeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeFilterComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                string tag = selectedItem.Tag?.ToString();

                if (tag == "ALL" || string.IsNullOrEmpty(tag))
                {
                    _selectedMacFilter = NodeSessionStateService.Instance.MacAddress;
                }
                else
                {
                    _selectedMacFilter = tag;
                }
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
                    SpikeHistory.Insert(0, new SpikeRecord
                    {
                        Timestamp = packet.Timestamp,
                        MacAddress = packet.MacAddress,
                        Value = packet.PayloadValue,
                        Unit = "°C"
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