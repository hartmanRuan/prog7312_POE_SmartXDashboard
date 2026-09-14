using Microsoft.Win32;
using QRCoder;
using SmartXDashboard.Models;
using SmartXDashboard.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SmartXDashboard
{
    public partial class SensorIngestionView : UserControl
    {
        private readonly TelemetryApiClient _apiClient = new TelemetryApiClient();
        private string _selectedFilePath = string.Empty;

        public SensorIngestionView()
        {
            InitializeComponent();
            this.Loaded += SensorIngestionView_Loaded;
        }

        private void SensorIngestionView_Loaded(object sender, RoutedEventArgs e)
        {
            // Restore state if a node was already registered or saved in this session
            if (NodeSessionStateService.Instance.IsNodeRegistered)
            {
                MacInput.Text = NodeSessionStateService.Instance.MacAddress;

                // Select matching zone if available
                if (!string.IsNullOrEmpty(NodeSessionStateService.Instance.LocationZone))
                {
                    foreach (ComboBoxItem item in ZoneComboBox.Items)
                    {
                        if (item.Content?.ToString() == NodeSessionStateService.Instance.LocationZone)
                        {
                            ZoneComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Lock down fields since only 1 node is permitted per user account
                MacInput.IsReadOnly = true;
                ZoneComboBox.IsEnabled = false;
                MetricComboBox.IsEnabled = false;

                // If a register button exists in XAML, disable it
                // RegisterButton.IsEnabled = false;
                StatusLabel.Text = "Status: Node already registered and persisted.";
                StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
            }
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Configuration Files (*.json;*.txt;*.log)|*.json;*.txt;*.log|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(_selectedFilePath);

                FileNameText.Text = fileInfo.Name;
                FileSizeText.Text = $"Size: {(fileInfo.Length / 1024.0):F1} KB";
                FileNameText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }

        private async void RegisterSensor_Click(object sender, RoutedEventArgs e)
        {
            string mac = MacInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(mac))
            {
                MessageBox.Show("Please enter a valid MAC Address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedZoneText = (ZoneComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Zone A";
            string selectedMetricText = (MetricComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Environmental";
            int userId = NodeSessionStateService.Instance.CurrentUserId;
            string barcode = $"BC-{mac}"; // Generate or map barcode value accordingly

            // Call backend API client to register node bound to current user
            bool isRegistered = await _apiClient.RegisterNodeAsync(userId, mac, barcode, selectedZoneText);
            if (!isRegistered)
            {
                MessageBox.Show("Failed to register node with the API server. Ensure you have not already registered a node or check your connection.", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Persist state to singleton service so it survives view navigation
            NodeSessionStateService.Instance.IsNodeRegistered = true;
            NodeSessionStateService.Instance.MacAddress = mac;
            NodeSessionStateService.Instance.BarcodeValue = barcode;
            NodeSessionStateService.Instance.LocationZone = selectedZoneText;

            // Upload configuration metadata file via multipart/form-data if selected
            if (!string.IsNullOrEmpty(_selectedFilePath))
            {
                bool isFileUploaded = await _apiClient.UploadNodeFileAsync(mac, _selectedFilePath);
                if (!isFileUploaded)
                {
                    MessageBox.Show("Node was registered, but the attached file failed to upload.", "Upload Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Generate dynamic payload string & barcode image
            string qrPayload = $"smartx://node?mac={mac}&zone={Uri.EscapeDataString(selectedZoneText)}&metric={Uri.EscapeDataString(selectedMetricText)}";

            BitmapImage qrBitmap = GenerateQrCodeBitmap(qrPayload);
            if (qrBitmap != null)
            {
                QrCodeImage.Source = qrBitmap;
                UnprovisionedPromptText.Visibility = Visibility.Collapsed;
                QrCodeImage.Visibility = Visibility.Visible;
            }

            // Update right panel dynamic overlay labels
            NodeIdLabel.Text = $"NODE ID: {mac}";
            StatusLabel.Text = "Status: Provisioned & Active";
            StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));

            // Lock down inputs after successful registration
            MacInput.IsReadOnly = true;
            ZoneComboBox.IsEnabled = false;
            MetricComboBox.IsEnabled = false;
        }

        private BitmapImage GenerateQrCodeBitmap(string payload)
        {
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeGraphic = qrCode.GetGraphic(20);

                        BitmapImage bitmap = new BitmapImage();
                        using (MemoryStream stream = new MemoryStream(qrCodeGraphic))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating QR code: {ex.Message}", "QR Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}