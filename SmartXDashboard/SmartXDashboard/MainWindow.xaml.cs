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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new SensorIngestionView());
        }

        private void NavProvisioning_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new SensorIngestionView());
        }

        private void NavTelemetry_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Children.Clear();
            MainContentFrame.Children.Add(new TelemetryStreamView());
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}