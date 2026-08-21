using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartXDashboard
{
    /// <summary>
    /// Interaction logic for SensorIngestionView.xaml
    /// </summary>
    public partial class SensorIngestionView : UserControl
    {
        private string selectedFilePath = string.Empty;

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
                FileInfo fileInfo = new FileInfo(selectedFilePath);

                FileNameLabel.Text = fileInfo.Name;
                FileNameLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                FileSizeLabel.Text = $"Size: {(fileInfo.Length / 1024.0):F2} KB";
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

            // Update Dynamic Engagement Panel Preview
            GeneratedNodeIdText.Text = $"NODE ID: {mac.ToUpper()}";
            StatusTagText.Text = "Status: Provisioned & Active";
            StatusTagText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#34C759"));

            MessageBox.Show($"Sensor Node [{mac}] successfully registered!", "Ingestion Pipeline", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
