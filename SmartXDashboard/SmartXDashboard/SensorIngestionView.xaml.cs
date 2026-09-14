using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;

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
            string selectedZone = (ZoneComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string selectedMetric = (MetricComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrWhiteSpace(mac))
            {
                MessageBox.Show("Please enter a valid MAC Address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Generate payload string for QR Code
            string qrPayload = $"smartx://node?mac={mac}&zone={Uri.EscapeDataString(selectedZone ?? "")}&metric={Uri.EscapeDataString(selectedMetric ?? "")}";

            BitmapImage qrBitmap = GenerateQrCodeBitmap(qrPayload);
            if (qrBitmap != null)
            {
                QrCodeImage.Source = qrBitmap;
                UnprovisionedPromptText.Visibility = Visibility.Collapsed;
                QrCodeImage.Visibility = Visibility.Visible;
            }

            // Update Right Side Status text
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