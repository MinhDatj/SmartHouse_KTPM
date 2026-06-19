using System.Windows;
using WPF_Shared.ViewModels;

namespace WPF_Manager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ManagerDashboardViewModel();
        }

        // Tạm ẩn lớp đỏ đi để quản lý nhìn thấy danh sách căn hộ báo lỗi
        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ManagerDashboardViewModel vm)
            {
                vm.HasEmergency = false;
            }
        }
    }
}