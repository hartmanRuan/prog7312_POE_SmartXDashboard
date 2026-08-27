using System;
using System.Collections.Generic;
using System.Linq;
using SmartXDashboard.Models;

namespace SmartXDashboard.Services
{
    public class AnalyticsMetrics
    {
        public int TotalPacketsReceived { get; set; }
        public int ActiveNodesCount { get; set; }
        public int WarningAlertsCount { get; set; }
        public int CriticalAlertsCount { get; set; }
        public double AveragePayloadValue { get; set; }
        public double PeakPayloadValue { get; set; }
    }

    public class AnalyticsService
    {
        /// <summary>
        /// Aggregates stream data using LINQ to compute real-time metrics for the UI dashboard.
        /// </summary>
        public AnalyticsMetrics CalculateMetrics(IEnumerable<TelemetryPacket<double>> packets)
        {
            if (packets == null || !packets.Any())
            {
                return new AnalyticsMetrics();
            }

            var packetList = packets.ToList();

            return new AnalyticsMetrics
            {
                TotalPacketsReceived = packetList.Count,
                ActiveNodesCount = packetList.Select(p => p.MacAddress).Distinct().Count(),
                WarningAlertsCount = packetList.Count(p => p.SeverityStatus == NodeStatus.Warning),
                CriticalAlertsCount = packetList.Count(p => p.SeverityStatus == NodeStatus.Critical),
                AveragePayloadValue = Math.Round(packetList.Average(p => p.PayloadValue), 2),
                PeakPayloadValue = Math.Round(packetList.Max(p => p.PayloadValue), 2)
            };
        }
    }
}