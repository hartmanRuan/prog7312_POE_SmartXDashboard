using SmartXDashboard.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SmartXDashboard.Services
{
    public class TelemetryApiClient
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5000/api/";

        public TelemetryApiClient()
        {
            _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<List<SensorNode>> GetNodesAsync()
        {
            try
            {
                return await _client.GetFromJsonAsync<List<SensorNode>>("nodes") ?? new List<SensorNode>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] GetNodes failed: {ex.Message}");
                return new List<SensorNode>();
            }
        }

        public async Task<List<TelemetryPacket<double>>> GetTelemetryAsync(string macFilter = "ALL")
        {
            try
            {
                return await _client.GetFromJsonAsync<List<TelemetryPacket<double>>>($"telemetry?macAddress={macFilter}")
                       ?? new List<TelemetryPacket<double>>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] GetTelemetry failed: {ex.Message}");
                return new List<TelemetryPacket<double>>();
            }
        }

        public async Task<bool> PostTelemetryAsync(TelemetryPacket<double> packet)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("telemetry", packet);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Bad Request Details: {errorContent}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Client Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterNodeAsync(SensorNode node)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("nodes", node);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] RegisterNode failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UploadMetadataFileAsync(string macAddress, string filePath)
        {
            if (!File.Exists(filePath)) return false;

            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            using var streamContent = new StreamContent(fileStream);

            content.Add(streamContent, "file", Path.GetFileName(filePath));

            var response = await _client.PostAsync($"nodes/{macAddress}/upload", content);
            return response.IsSuccessStatusCode;
        }
    }
}