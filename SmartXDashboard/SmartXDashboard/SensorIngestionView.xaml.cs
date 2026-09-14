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
        private string _selectedFilePath = string.Empty;

        public SensorIngestionView()
        {
            InitializeComponent();
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

        private void RegisterSensor_Click(object sender, RoutedEventArgs e)
        {
            string mac = MacInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(mac))
            {
                MessageBox.Show("Please enter a valid MAC Address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Map ComboBox selections to Model Enums
            ZoneLocation zone = ZoneComboBox.SelectedIndex switch
            {
                0 => ZoneLocation.ZoneA_Environmental,
                1 => ZoneLocation.ZoneB_PowerGrid,
                2 => ZoneLocation.ZoneC_ActuatorControl,
                _ => ZoneLocation.ZoneA_Environmental
            };

            SensorCategory category = MetricComboBox.SelectedIndex switch
            {
                0 => SensorCategory.Environmental,
                1 => SensorCategory.Electrical,
                2 => SensorCategory.Mechanical,
                _ => SensorCategory.Environmental
            };

            // 2. Instantiate SensorNode matching your model properties
            var newSensor = new SensorNode
            {
                MacAddress = mac,
                NodeId = mac,
                LocationZone = zone,
                Category = category,
                Status = NodeStatus.Active,
                ProvisionedTimestamp = DateTime.Now
            };

            // If a configuration metadata file was browsed and selected, attach its details
            if (!string.IsNullOrEmpty(_selectedFilePath))
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(_selectedFilePath);
                newSensor.ConfigFileName = fileInfo.Name;
                newSensor.ConfigFileSizeKB = fileInfo.Length / 1024;
            }

            // 3. Save node into local memory bus
            SensorRepository.Instance.AddNode(newSensor);

            // 4. Generate dynamic payload string & barcode image
            string selectedZoneText = (ZoneComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string selectedMetricText = (MetricComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string qrPayload = $"smartx://node?mac={mac}&zone={Uri.EscapeDataString(selectedZoneText)}&metric={Uri.EscapeDataString(selectedMetricText)}";

            BitmapImage qrBitmap = GenerateQrCodeBitmap(qrPayload);
            if (qrBitmap != null)
            {
                QrCodeImage.Source = qrBitmap;
                UnprovisionedPromptText.Visibility = Visibility.Collapsed;
                QrCodeImage.Visibility = Visibility.Visible;
            }

            // 5. Update right panel dynamic overlay labels
            NodeIdLabel.Text = $"NODE ID: {mac}";
            StatusLabel.Text = "Status: Provisioned & Active";
            StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
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