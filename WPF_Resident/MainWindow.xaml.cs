using WPF_Shared.ViewModels;
using System.Windows;

namespace WPF_Resident
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Gán ViewModel cho View (Bước rất quan trọng của MVVM)
            this.DataContext = new ResidentDashboardViewModel();
        }
    }
}