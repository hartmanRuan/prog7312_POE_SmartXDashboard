using SmartX.Api.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SmartX.Api.Repositories
{
    public sealed class SensorRepository
    {
        private static readonly Lazy<SensorRepository> _instance = new(() => new SensorRepository());
        public static SensorRepository Instance => _instance.Value;

        private readonly ConcurrentBag<TelemetryPacket<double>> _telemetryLogs = new();
        private readonly ConcurrentBag<SensorNode> _registeredNodes = new();

        private SensorRepository() { }

        public void AddTelemetry(TelemetryPacket<double> packet)
        {
            if (packet != null)
            {
                _telemetryLogs.Add(packet);

                // Automatically register the node when a packet arrives
                if (!string.IsNullOrEmpty(packet.MacAddress))
                {
                    RegisterNode(new SensorNode
                    {
                        MacAddress = packet.MacAddress,
                        LocationZone = packet.LocationZone
                    });
                }
            }
        }

        public IEnumerable<TelemetryPacket<double>> GetTelemetry() => _telemetryLogs;

        public void RegisterNode(SensorNode node)
        {
            if (node != null && !string.IsNullOrEmpty(node.MacAddress))
            {
                if (!_registeredNodes.Any(n => n.MacAddress.Equals(node.MacAddress, StringComparison.OrdinalIgnoreCase)))
                {
                    _registeredNodes.Add(node);
                }
            }
        }

        public IEnumerable<SensorNode> GetNodes() => _registeredNodes;
    }
}