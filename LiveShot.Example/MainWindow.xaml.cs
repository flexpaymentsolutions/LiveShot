using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
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
using LiveShot.API;
using LiveShot.UI;
using LiveShot.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveShot.Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            IServiceProvider? ServiceProvider;

            IConfiguration? Configuration;

            CaptureScreenView? CaptureScreenView;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Properties/appsettings.json", false, true);

            Configuration = builder.Build();
            string? culture = Configuration?["CultureUI"];

            if (culture is not null) Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            var serviceCollection = new ServiceCollection()
                .ConfigureAPI(Configuration)
                .ConfigureUI();

            if (Configuration != null)
                serviceCollection.AddSingleton(Configuration);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            CaptureScreenView = ServiceProvider.GetRequiredService<CaptureScreenView>();

            CaptureScreenView.CaptureScreen();
            CaptureScreenView.Show();
            CaptureScreenView.Activate();
            CaptureScreenView.Focus();

            CaptureScreenView.Closing += CaptureScreenView_Closing;
        }

        private void CaptureScreenView_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
        }
    }
}