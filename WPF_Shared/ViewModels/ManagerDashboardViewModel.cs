using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPF_Shared.Services;

namespace WPF_Shared.ViewModels;

// 1. MODEL CĂN HỘ
public partial class ApartmentModel : ObservableObject
{
    [ObservableProperty] private string id = string.Empty;
    [ObservableProperty] private string apartmentName = string.Empty;
    [ObservableProperty] private bool isNormal = true; // true = Ổn, false = Bất thường
}

// 2. MODEL CẢNH BÁO / YÊU CẦU HỖ TRỢ
public partial class AlertModel : ObservableObject
{
    [ObservableProperty] private string alertId = Guid.NewGuid().ToString();
    [ObservableProperty] private string apartmentName = string.Empty;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string time = DateTime.Now.ToString("HH:mm:ss");
}

// 3. VIEWMODEL CHÍNH CỦA QUẢN LÝ
public partial class ManagerDashboardViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ApartmentModel> apartments = new();
    [ObservableProperty] private ObservableCollection<AlertModel> activeAlerts = new();

    // Cờ kích hoạt màn hình đỏ Pop-up
    [ObservableProperty] private bool hasEmergency;

    public ManagerDashboardViewModel()
    {
        // Khởi tạo danh sách căn hộ
        Apartments.Add(new ApartmentModel { Id = "P101", ApartmentName = "Căn hộ 101", IsNormal = true });
        Apartments.Add(new ApartmentModel { Id = "P102", ApartmentName = "Căn hộ 102", IsNormal = true });
        Apartments.Add(new ApartmentModel { Id = "P103", ApartmentName = "Căn hộ 103", IsNormal = true });
        Apartments.Add(new ApartmentModel { Id = "P201", ApartmentName = "Căn hộ 201", IsNormal = true });
        Apartments.Add(new ApartmentModel { Id = "P202", ApartmentName = "Căn hộ 202", IsNormal = true });

        //StartMockEmergencySimulator();
        // Khởi tạo SignalR Service
        var signalR = new SignalRService();

        // Đăng ký hứng báo động (Để màn hình Quản lý đỏ rực lên)
        signalR.OnAlertTriggered += (apartmentId, apartmentName, message) =>
        {
            string uiCardId = $"P10{apartmentId}";
            OnAlertReceived(uiCardId, apartmentName, message); // Gọi đúng hàm ta đã sửa ở bài trước
        };

        // Kích hoạt chạy ngầm
        _ = signalR.ConnectAsync();
    }

    // --- GIẢ LẬP NHẬN TÍN HIỆU TỪ CHỦ HỘ ---
    private void StartMockEmergencySimulator()
    {
        Task.Run(async () =>
        {
            Random rnd = new Random();
            await Task.Delay(5000); // Đợi 5s sau khi mở app mới bắt đầu test

            while (true)
            {
                // Giả lập căn hộ 102 gặp sự cố bất thường
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var p102 = Apartments.FirstOrDefault(a => a.Id == "P102");
                    if (p102 != null) p102.IsNormal = false; // Đổi trạng thái sang Bất thường

                    // Thêm thông báo vào trung tâm hỗ trợ
                    ActiveAlerts.Insert(0, new AlertModel
                    {
                        ApartmentName = "Căn hộ 102",
                        Message = "Phát hiện nhiệt độ cao / Yêu cầu cứu hộ khẩn cấp!"
                    });

                    // Bật cờ nhấp nháy màn hình đỏ
                    HasEmergency = true;
                });

                await Task.Delay(15000); // 15s sinh ra 1 lỗi để test
            }
        });
    }

    // --- HÀM CHUẨN BỊ HỨNG DỮ LIỆU SIGNALR TỪ SERVER ---
    public void OnAlertReceived(string uiCardId, string apartmentName, string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var apt = Apartments.FirstOrDefault(a => a.Id == uiCardId);
            if (apt != null) apt.IsNormal = false;

            ActiveAlerts.Insert(0, new AlertModel
            {
                ApartmentName = apartmentName, 
                Message = message
            });

            HasEmergency = true;
        });
    }

    // --- COMMAND: GỬI TIN NHẮN CHO CHỦ HỘ ---
    [RelayCommand]
    private void SendMessageToResident(ApartmentModel targetApartment)
    {
        // Thực tế sẽ gọi API (HTTP POST) hoặc SignalR Hub tới Server
        MessageBox.Show($"Đã gửi tin nhắn cảnh báo nhắc nhở tới chủ hộ {targetApartment.ApartmentName}.",
            "Gửi tín hiệu thành công", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // --- COMMAND: XÁC NHẬN ĐÃ TIẾP NHẬN HỖ TRỢ ---
    [RelayCommand]
    private void AcknowledgeAlert(AlertModel alert)
    {
        // 1. Xóa cảnh báo khỏi danh sách
        ActiveAlerts.Remove(alert);

        // 2. Tắt Pop-up đỏ nếu hết cảnh báo
        if (ActiveAlerts.Count == 0)
        {
            HasEmergency = false;
        }

        // 3. Phục hồi trạng thái căn hộ về bình thường (Tùy logic nghiệp vụ)
        var apt = Apartments.FirstOrDefault(a => a.ApartmentName == alert.ApartmentName);
        if (apt != null) apt.IsNormal = true;

        // 4. Gửi tín hiệu phản hồi về cho app chủ hộ
        // Thực tế: Gửi MQTT Publish báo ban quản lý đang lên
        MessageBox.Show($"Đã gửi tín hiệu phản hồi: 'Ban quản lý đã tiếp nhận và đang xử lý' về app của {alert.ApartmentName}.",
            "Đã tiếp nhận", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}