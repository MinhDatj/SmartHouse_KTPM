using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;

namespace WPF_Shared.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    // Sự kiện (Delegate) để thông báo cho View biết khi nào đăng nhập thành công
    public Action? OnLoginSuccess { get; set; }

    // Command xử lý đăng nhập, nhận tham số là ô PasswordBox từ giao diện
    [RelayCommand]
    private void Login(object parameter)
    {
        // Lấy control PasswordBox từ tham số truyền vào
        if (parameter is PasswordBox passwordBox)
        {
            string password = passwordBox.Password;

            // KIỂM TRA ĐĂNG NHẬP (Giả lập khi chưa có DB)
            // Bạn có thể thay đổi "admin" và "123" thành tài khoản bạn muốn test
            if (Username == "admin" && password == "123")
            {
                HasError = false;
                ErrorMessage = string.Empty;

                // Kích hoạt sự kiện chuyển màn hình
                OnLoginSuccess?.Invoke();
            }
            else
            {
                HasError = true;
                ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác. Vui lòng thử lại!";

                // Xóa trắng ô mật khẩu để người dùng nhập lại
                passwordBox.Password = string.Empty;
            }
        }
    }
}