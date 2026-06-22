using System.Windows;
using WPF_Shared.ViewModels;

namespace WPF_Manager
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var viewModel = new LoginViewModel(); // Dùng chung logic đăng nhập
            viewModel.OnLoginSuccess = () =>
            {
                MainWindow dashboard = new MainWindow();
                dashboard.Show();
                this.Close();
            };
            this.DataContext = viewModel;
        }
    }
}