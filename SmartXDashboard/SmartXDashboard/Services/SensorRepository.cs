using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SmartXDashboard.Models;

namespace SmartXDashboard.Services
{
    public class SensorRepository
    {
        private static readonly Lazy<SensorRepository> _instance =
            new Lazy<SensorRepository>(() => new SensorRepository());

        public static SensorRepository Instance => _instance.Value;

        // Thread-safe dictionary using MAC Address as the unique key
        private readonly ConcurrentDictionary<string, SensorNode> _nodes = new ConcurrentDictionary<string, SensorNode>();

        private SensorRepository()
        {
            SeedInitialNodes();
        }

        public bool RegisterNode(SensorNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.MacAddress))
                return false;

            return _nodes.TryAdd(node.MacAddress.ToUpper(), node);
        }

        public IEnumerable<SensorNode> GetAllNodes()
        {
            return _nodes.Values.ToList();
        }

        public SensorNode GetNodeByMac(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
                return null;

            _nodes.TryGetValue(macAddress.ToUpper(), out var node);
            return node;
        }

        public bool RemoveNode(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
                return false;

            return _nodes.TryRemove(macAddress.ToUpper(), out _);
        }

        private void SeedInitialNodes()
        {
            var seedNodes = new[]
            {
                new SensorNode("00:1A:2C:3D:4E:5F", ZoneLocation.ZoneA_Environmental, SensorCategory.Environmental, "env_config.json", 12),
                new SensorNode("00:1A:2C:3D:4E:6A", ZoneLocation.ZoneB_PowerGrid, SensorCategory.Electrical, "grid_spec.txt", 8),
                new SensorNode("00:1A:2C:3D:4E:7B", ZoneLocation.ZoneC_ActuatorControl, SensorCategory.Mechanical, "actuator.log", 24)
            };

            foreach (var node in seedNodes)
            {
                _nodes.TryAdd(node.MacAddress.ToUpper(), node);
            }
        }
    }
}