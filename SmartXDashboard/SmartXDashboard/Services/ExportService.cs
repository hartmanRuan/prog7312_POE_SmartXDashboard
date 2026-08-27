using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SmartXDashboard.Models;

namespace SmartXDashboard.Services
{
    public class ExportService
    {
        /// <summary>
        /// Exports a list of telemetry packets to a CSV file.
        /// </summary>
        public bool ExportTelemetryToCsv(IEnumerable<TelemetryPacket<double>> packets, string destinationFilePath)
        {
            if (packets == null || string.IsNullOrWhiteSpace(destinationFilePath))
                return false;

            try
            {
                var sb = new StringBuilder();

                // Write CSV Header
                sb.AppendLine("Timestamp,MAC Address,Location Zone,Payload Value,Severity Status");

                foreach (var packet in packets)
                {
                    string line = $"\"{packet.Timestamp:yyyy-MM-dd HH:mm:ss.fff}\",\"{packet.MacAddress}\",\"{packet.LocationZone}\",{packet.PayloadValue},\"{packet.SeverityStatus}\"";
                    sb.AppendLine(line);
                }

                File.WriteAllText(destinationFilePath, sb.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a formatted text summary report of system telemetry metrics.
        /// </summary>
        public bool ExportAnalyticsReport(AnalyticsMetrics metrics, string destinationFilePath)
        {
            if (metrics == null || string.IsNullOrWhiteSpace(destinationFilePath))
                return false;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("==========================================");
                sb.AppendLine("      SMART-X DASHBOARD ANALYTICS REPORT  ");
                sb.AppendLine("==========================================");
                sb.AppendLine($"Generated On: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Total Packets Processed: {metrics.TotalPacketsReceived}");
                sb.AppendLine($"Unique Active Nodes:     {metrics.ActiveNodesCount}");
                sb.AppendLine($"Average Payload Value:   {metrics.AveragePayloadValue:F2}");
                sb.AppendLine($"Peak Payload Value:      {metrics.PeakPayloadValue:F2}");
                sb.AppendLine($"Warning Level Alerts:    {metrics.WarningAlertsCount}");
                sb.AppendLine($"Critical Level Alerts:   {metrics.CriticalAlertsCount}");
                sb.AppendLine("==========================================");

                File.WriteAllText(destinationFilePath, sb.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}