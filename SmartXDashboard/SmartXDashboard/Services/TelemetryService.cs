using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SmartXDashboard.Models;

namespace SmartXDashboard.Services
{
    public class TelemetrySimulator
    {
        // Explicitly define System.Timers.Timer to resolve ambiguity
        private readonly System.Timers.Timer _timer;
        private readonly Random _random = new Random();

        // Event raised every time a new simulated packet arrives
        public event Action<TelemetryPacket<double>> OnTelemetryReceived;

        public bool IsRunning => _timer.Enabled;

        public TelemetrySimulator(double intervalMilliseconds = 2000)
        {
            _timer = new System.Timers.Timer(intervalMilliseconds);
            _timer.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var nodes = SensorRepository.Instance.GetAllNodes().ToList();
            if (!nodes.Any())
                return;

            // Pick a random registered node to simulate
            var selectedNode = nodes[_random.Next(nodes.Count)];

            // Generate realistic values based on category
            double payloadValue;
            string unit;

            switch (selectedNode.Category)
            {
                case SensorCategory.Electrical:
                    payloadValue = Math.Round(220 + (_random.NextDouble() * 20 - 10), 2); // 210V - 230V
                    unit = "V";
                    break;
                case SensorCategory.Mechanical:
                    payloadValue = _random.Next(100, 1500); // RPM or pulse counts
                    unit = "RPM";
                    break;
                case SensorCategory.Environmental:
                default:
                    payloadValue = Math.Round(18 + (_random.NextDouble() * 14), 2); // 18°C - 32°C
                    unit = "°C";
                    break;
            }

            // Determine status severity based on value thresholds
            NodeStatus status = NodeStatus.Active;
            if (unit == "°C" && payloadValue > 29.5)
            {
                status = NodeStatus.Warning;
            }
            else if (unit == "V" && payloadValue < 212)
            {
                status = NodeStatus.Critical;
            }

            var packet = new TelemetryPacket<double>(
                selectedNode.MacAddress,
                selectedNode.LocationZone,
                payloadValue,
                unit,
                status
            );

            // Raise the event on the background thread
            OnTelemetryReceived?.Invoke(packet);
        }
    }
}