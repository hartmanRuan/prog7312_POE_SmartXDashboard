using SmartXDashboard.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace SmartXDashboard.Services
{
    public class TelemetrySimulator
    {
        private System.Threading.Timer? _timer;
        private readonly TelemetryApiClient _apiClient = new();
        private readonly Random _random = new();

        public void Start()
        {
            //MessageBox.Show("Simulator Start() called!"); // Check if Start is hit

            _timer = new System.Threading.Timer(async _ =>
            {
                //MessageBox.Show("Timer tick fired!"); // Check if the timer is ticking

                try
                {
                    var packet = new TelemetryPacket<double>
                    {
                        MacAddress = "00:1A:2B:3C:4D:5E",
                        PayloadValue = Math.Round(20.0 + (_random.NextDouble() * 15.0), 2),
                        Timestamp = DateTime.Now,
                        LocationZone = ZoneLocation.ZoneA_Environmental,
                        MetricUnit = "°C",
                        SeverityStatus = NodeStatus.Active
                    };

                    bool success = await _apiClient.PostTelemetryAsync(packet);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Simulator Error: {ex.Message}");
                }
            }, null, 0, 1500);
        }

        public void Stop()
        {
            _timer?.Dispose();
        }
    }
}