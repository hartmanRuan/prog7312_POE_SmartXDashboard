using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SmartXDashboard.Models;
using SmartXDashboard.Services;

namespace SmartXDashboard
{
    public partial class TelemetryStreamView : UserControl
    {
        private readonly TelemetrySimulator _simulator;
        public ObservableCollection<TelemetryPacket<double>> TelemetryStream { get; set; }

        public TelemetryStreamView()
        {
            InitializeComponent();

            TelemetryStream = new ObservableCollection<TelemetryPacket<double>>();

            if (TelemetryGrid != null)
            {
                TelemetryGrid.ItemsSource = TelemetryStream;
            }

            // Initialize simulator to push telemetry every 1.5 seconds
            _simulator = new TelemetrySimulator(1500);
            _simulator.OnTelemetryReceived += Simulator_OnTelemetryReceived;

            // Start/Stop timer with view lifecycle
            Loaded += (s, e) => _simulator.Start();
            Unloaded += (s, e) => _simulator.Stop();
        }

        private void Simulator_OnTelemetryReceived(TelemetryPacket<double> packet)
        {
            // Marshal background thread execution to WPF UI Thread
            Dispatcher.Invoke(() =>
            {
                if (TelemetryStream.Count >= 50)
                {
                    TelemetryStream.RemoveAt(TelemetryStream.Count - 1);
                }

                TelemetryStream.Insert(0, packet);
            });
        }

        // Manual trigger button handler (kept for manual override testing)
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

            TelemetryStream.Insert(0, packet);
        }

        private void SearchMacInput_TextChanged(object sender, TextChangedEventArgs e) { }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    }
}