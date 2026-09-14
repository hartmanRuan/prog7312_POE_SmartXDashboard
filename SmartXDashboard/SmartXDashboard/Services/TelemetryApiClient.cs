using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using SmartXDashboard.Models;
using System.Windows;
using System.IO;

namespace SmartXDashboard.Services
{
    public class TelemetryApiClient
    {
        private readonly HttpClient _httpClient;

        public TelemetryApiClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5000/") // Match your backend port
            };
        }

        public async Task<AuthResponse?> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { username, password });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<bool> RegisterNodeAsync(int userId, string macAddress, string barcode, string locationZone)
        {
            var payload = new { userId, macAddress, barcode, locationZone };
            var response = await _httpClient.PostAsJsonAsync("api/nodes/register", new { userId, macAddress, barcode, locationZone });

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                System.Windows.MessageBox.Show($"Node Register Error ({response.StatusCode}): {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<List<NodeModel>> GetActiveNodesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<NodeModel>>("nodes") ?? new();
        }

        public async Task<List<TelemetryPacket<double>>> GetTelemetryAsync(string macAddress)
        {
            var response = await _httpClient.GetAsync($"api/telemetry/{macAddress}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<TelemetryPacket<double>>>() ?? new();
            }
            return new();
        }
        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            var payload = new { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", payload);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                System.Windows.MessageBox.Show($"Status: {response.StatusCode}\nBody: '{errorContent}'");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PostTelemetryAsync<T>(TelemetryPacket<T> packet)
        {
            var response = await _httpClient.PostAsJsonAsync("telemetry", packet);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> UploadNodeFileAsync(string macAddress, string filePath)
        {
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            using var streamContent = new StreamContent(fileStream);

            form.Add(streamContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync($"api/nodes/{macAddress}/upload", form);
            return response.IsSuccessStatusCode;
        }

        public class AuthResponse
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public bool HasNode { get; set; }
            public NodeModel? Node { get; set; }
        }

        public class NodeModel
        {
            public int Id { get; set; }
            public string MacAddress { get; set; } = string.Empty;
            public string Barcode { get; set; } = string.Empty;
            public string LocationZone { get; set; } = string.Empty;
        }
    }
}