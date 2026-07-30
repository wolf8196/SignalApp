using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SignalApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Services.GetRequiredService<MainViewModel>();
        }
    }
}