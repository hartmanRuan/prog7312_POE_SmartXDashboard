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
        private int _tickCounter = 0;

        public void Start()
        {
            _timer = new System.Threading.Timer(async _ =>
            {
                try
                {
                    _tickCounter++;
                    double payload = Math.Round(20.0 + (_random.NextDouble() * 15.0), 2);

                    // Inject a clear spike over the 85.0 threshold every 5th tick
                    if (_tickCounter % 5 == 0)
                    {
                        payload = 95.0; // Force it well above the 85.0 threshold for testing
                    }

                    var packet = new TelemetryPacket<double>
                    {
                        MacAddress = "00:1A:2B:3C:4D:5E",
                        PayloadValue = payload,
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