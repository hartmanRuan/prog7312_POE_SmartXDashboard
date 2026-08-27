using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SmartXDashboard.Models;
using SmartXDashboard.Services;

namespace SmartXDashboard
{
    /// <summary>
    /// Interaction logic for SensorIngestionView.xaml
    /// </summary>
    public partial class SensorIngestionView : UserControl
    {
        private string selectedFilePath = string.Empty;
        private readonly ConfigFileParser _configParser = new ConfigFileParser();
        private readonly BarcodeService _barcodeService = new BarcodeService();

        public SensorIngestionView()
        {
            InitializeComponent();
        }

        private void BrowseConfigFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Sensor Configuration Metadata File",
                Filter = "Config Files (*.json;*.txt;*.log)|*.json;*.txt;*.log|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;

                // Parse configuration file metadata (Commit 5)
                var configMetadata = _configParser.ParseFile(selectedFilePath);

                FileNameLabel.Text = configMetadata.FileName;
                FileNameLabel.Foreground = new SolidColorBrush(Colors.White);
                FileSizeLabel.Text = $"Size: {configMetadata.FileSizeKB} KB ({configMetadata.PropertyCount} keys)";
            }
        }

        private void RegisterSensor_Click(object sender, RoutedEventArgs e)
        {
            string mac = MacAddressInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(mac))
            {
                MessageBox.Show("Please enter a valid MAC address or Node ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Map UI ComboBox selections to Enums safely
            ZoneLocation zone = ZoneLocation.ZoneA_Environmental;
            if (ZoneComboBox != null && ZoneComboBox.SelectedIndex >= 0)
            {
                zone = (ZoneLocation)ZoneComboBox.SelectedIndex;
            }

            SensorCategory category = SensorCategory.Environmental;
            if (CategoryComboBox != null && CategoryComboBox.SelectedIndex >= 0)
            {
                category = (SensorCategory)CategoryComboBox.SelectedIndex;
            }

            // Parse file metadata
            var configMetadata = _configParser.ParseFile(selectedFilePath);

            // Create model and register into memory repository (Commit 1 & 3)
            SensorNode node = new SensorNode(mac, zone, category, configMetadata.FileName, configMetadata.FileSizeKB);
            bool success = SensorRepository.Instance.RegisterNode(node);

            if (success)
            {
                // Update Dynamic Engagement Panel Preview
                GeneratedNodeIdText.Text = node.NodeId;
                StatusTagText.Text = "Status: Provisioned & Active";
                StatusTagText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34C759"));

                // Generate and render QR Barcode (Commit 6 & 7)
                var barcodeImage = _barcodeService.GenerateBarcodeImage(node.MacAddress);
                if (barcodeImage != null && BarcodePreviewImage != null)
                {
                    BarcodePreviewImage.Source = barcodeImage;
                }

                MessageBox.Show($"Sensor Node [{node.NodeId}] successfully registered in repository!", "Ingestion Pipeline", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"A node with MAC address [{mac}] already exists in the repository.", "Duplicate Node", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}