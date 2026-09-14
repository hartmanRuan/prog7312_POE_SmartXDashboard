using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartXDashboard.Models;
using SmartXDashboard.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmartXDashboard
{
    public partial class TelemetryStreamView : UserControl
    {
        private readonly TelemetrySimulator _simulator;
        private readonly ObservableCollection<TelemetryPacket<double>> _telemetryLog = new ObservableCollection<TelemetryPacket<double>>();
        private readonly ObservableCollection<double> _chartValues = new ObservableCollection<double>();
        private string _selectedMacFilter = "ALL";

        public ISeries[] ChartSeries { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        public TelemetryStreamView()
        {
            InitializeComponent();
            DataContext = this;

            TelemetryDataGrid.ItemsSource = _telemetryLog;

            // Initialize LiveCharts2 Series with #FFC107 Gold Line styling
            ChartSeries = new ISeries[]
            {
        new LineSeries<double>
        {
            Values = _chartValues,
            Fill = null,
            Stroke = new SolidColorPaint(SKColor.Parse("#FFC107")) { StrokeThickness = 2 },
            GeometrySize = 6,
            GeometryStroke = new SolidColorPaint(SKColor.Parse("#FFC107")),
            GeometryFill = new SolidColorPaint(SKColor.Parse("#181818"))
        }
            };

            XAxes = new Axis[]
            {
        new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.Gray),
            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#222222"))
        }
            };

            YAxes = new Axis[]
            {
        new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.Gray),
            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#222222"))
        }
            };

            // Instantiate chart directly in C# to eliminate XAML designer namespace errors
            var cartesianChart = new LiveChartsCore.SkiaSharpView.WPF.CartesianChart
            {
                Series = ChartSeries,
                XAxes = XAxes,
                YAxes = YAxes,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden
            };

            ChartContainer.Children.Add(cartesianChart);

            _simulator = new TelemetrySimulator();
            _simulator.OnTelemetryReceived += Simulator_OnTelemetryReceived;

            Loaded += TelemetryStreamView_Loaded;
            Unloaded += TelemetryStreamView_Unloaded;
        }

        private void TelemetryStreamView_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateNodeFilterDropdown();
            _simulator.Start();
        }

        private void TelemetryStreamView_Unloaded(object sender, RoutedEventArgs e)
        {
            _simulator.Stop();
        }

        private void PopulateNodeFilterDropdown()
        {
            NodeFilterComboBox.Items.Clear();

            ComboBoxItem allItem = new ComboBoxItem { Content = "All Registered Nodes", Tag = "ALL", Foreground = System.Windows.Media.Brushes.Black, IsSelected = true };
            NodeFilterComboBox.Items.Add(allItem);

            var registeredNodes = SensorRepository.Instance.GetAllNodes().ToList();
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

        private void NodeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _selectedMacFilter = selectedItem.Tag?.ToString() ?? "ALL";

                // Null guard prevents crash during initial component setup
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

        private void Simulator_OnTelemetryReceived(TelemetryPacket<double> packet)
        {
            Dispatcher.Invoke(() =>
            {
                if (EmptyChartPrompt != null && EmptyChartPrompt.Visibility == Visibility.Visible)
                {
                    EmptyChartPrompt.Visibility = Visibility.Collapsed;
                }

                if (_selectedMacFilter == "ALL" || packet.MacAddress == _selectedMacFilter)
                {
                    _telemetryLog.Insert(0, packet);
                    if (_telemetryLog.Count > 100)
                    {
                        _telemetryLog.RemoveAt(_telemetryLog.Count - 1);
                    }

                    // Push value to live timeline chart
                    _chartValues.Add(packet.PayloadValue);
                    if (_chartValues.Count > 20)
                    {
                        _chartValues.RemoveAt(0);
                    }
                }
            });
        }
    }
}