using System.Windows;
using WPF_Shared.ViewModels; // Gọi ViewModel dùng chung

namespace WPF_Resident
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Khởi tạo ViewModel
            var viewModel = new LoginViewModel();

            // Lắng nghe sự kiện OnLoginSuccess từ ViewModel
            viewModel.OnLoginSuccess = () =>
            {
                // Mở cửa sổ Dashboard chính
                MainWindow dashboard = new MainWindow();
                dashboard.Show();

                // Đóng cửa sổ đăng nhập hiện tại
                this.Close();
            };

            // Gán ViewModel cho View
            this.DataContext = viewModel;
        }
    }
}