using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using WPF_Shared.Services;

namespace WPF_Shared.ViewModels;

// 1. MODEL THÔNG BÁO TỪ BAN QUẢN LÝ
public partial class NotificationModel : ObservableObject
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string time = string.Empty;
    [ObservableProperty] private bool isRead = false;
}

// 2. VIEWMODEL CHÍNH (ĐÃ LƯỢC BỎ PHẦN THIẾT BỊ)
public partial class ResidentDashboardViewModel : ObservableObject
{
    // --- CÁC BIẾN CỦA TAB TỔNG QUAN ---
    [ObservableProperty] private double currentTemperature;
    [ObservableProperty] private double currentHumidity;
    [ObservableProperty] private double currentSmoke;

    [ObservableProperty] private bool isTempDanger;
    [ObservableProperty] private bool isHumDanger;
    [ObservableProperty] private bool isSmokeDanger;

    [ObservableProperty] private string selectedMetric = "Temperature";
    [ObservableProperty] private string selectedTimeFrame = "1h";
    [ObservableProperty] private string xAxisTitle = "Phút trước";

    [ObservableProperty] private SeriesCollection historyChartSeries;
    [ObservableProperty] private ObservableCollection<string> chartLabels;

    // Để lưu trữ ngầm, tránh reset đồ thị khi chuyển sang xem cái khác
    private List<double> _tempHistory = new List<double>();
    private List<double> _humHistory = new List<double>();
    private List<double> _smokeHistory = new List<double>();
    private List<string> _timeHistory = new List<string>();

    // --- CÁC BIẾN CHO TAB THÔNG BÁO ---
    [ObservableProperty] private ObservableCollection<NotificationModel> notifications;

    private readonly SignalRService _signalRService;

    public ResidentDashboardViewModel()
    {
        HistoryChartSeries = new SeriesCollection();
        ChartLabels = new ObservableCollection<string>();
        Notifications = new ObservableCollection<NotificationModel>();

        _ = LoadChartDataAsync();

        _signalRService = new SignalRService();

        _signalRService.OnRealTimeDataUpdated += (temp, hum, smoke) => { OnRealTimeDataReceived(temp, hum, smoke); };
        _signalRService.OnAlertTriggered += (apartmentId, apartmentName, message) => { ReceiveNotificationFromManager("BÁO ĐỘNG KHẨN CẤP", message); };

        _ = _signalRService.ConnectAsync();
    }

    // --- KHỞI TẠO DỮ LIỆU THÔNG BÁO GIẢ LẬP ---
    private void InitializeMockNotifications()
    {
        Notifications.Clear();

        // Kịch bản 1: Quản lý thấy bất thường và gửi nhắc nhở xuống
        Notifications.Add(new NotificationModel
        {
            Title = "CẢNH BÁO TỪ BAN QUẢN LÝ",
            Message = "Hệ thống trung tâm ghi nhận nhiệt độ tại căn hộ của bạn đang tăng cao bất thường. Vui lòng kiểm tra ngay lập tức. Kỹ thuật viên đang di chuyển lên hỗ trợ.",
            Time = DateTime.Now.AddMinutes(-5).ToString("HH:mm dd/MM/yyyy"),
            IsRead = false
        });

        // Kịch bản 2: Quản lý xác nhận đã nhận yêu cầu hỗ trợ của Chủ hộ
        Notifications.Add(new NotificationModel
        {
            Title = "ĐÃ TIẾP NHẬN YÊU CẦU CỨU HỘ",
            Message = "Ban quản lý đã nhận được tín hiệu yêu cầu hỗ trợ khẩn cấp từ ứng dụng của bạn. Đội an ninh và kỹ thuật đang tiếp cận căn hộ để xử lý.",
            Time = DateTime.Now.AddMinutes(-1).ToString("HH:mm dd/MM/yyyy"),
            IsRead = false
        });
    }

    // --- HÀM NHẬN TIN NHẮN THỰC TẾ QUA MQTT TRONG TƯƠNG LAI ---
    public void ReceiveNotificationFromManager(string title, string content)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Insert(0, new NotificationModel
            {
                Title = title,
                Message = content,
                Time = DateTime.Now.ToString("HH:mm dd/MM/yyyy"),
                IsRead = false
            });
        });
    }

    // --- COMMAND: ĐÁNH DẤU ĐÃ ĐỌC ---
    [RelayCommand]
    private void MarkAsRead(NotificationModel notif)
    {
        if (notif != null)
        {
            notif.IsRead = true;
        }
    }

    // --- MOCK SENSOR DATA RUNNER ---
    private void StartMockDataSimulator()
    {
        Task.Run(async () =>
        {
            Random rnd = new Random();
            while (true)
            {
                double mockTemp = Math.Round(rnd.NextDouble() * 45 + 20, 1);
                double mockHum = Math.Round(rnd.NextDouble() * 45 + 50, 1);
                double mockSmoke = Math.Round(rnd.NextDouble() * 500 + 100, 1);

                OnRealTimeDataReceived(mockTemp, mockHum, mockSmoke);
                await Task.Delay(3000);
            }
        });
    }

    public void OnRealTimeDataReceived(double temp, double hum, double smoke)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            CurrentTemperature = temp;
            CurrentHumidity = hum;
            CurrentSmoke = smoke;

            IsTempDanger = temp > 60;
            IsHumDanger = hum > 90;
            IsSmokeDanger = smoke > 500;

            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            // 1. Nạp đạn vào cả 3 KHO NGẦM
            _tempHistory.Add(temp);
            _humHistory.Add(hum);
            _smokeHistory.Add(smoke);
            _timeHistory.Add(currentTime);

            // 2. Cơ chế dịch chuyển 15 điểm cho KHO NGẦM
            if (_tempHistory.Count > 15)
            {
                _tempHistory.RemoveAt(0);
                _humHistory.RemoveAt(0);
                _smokeHistory.RemoveAt(0);
                _timeHistory.RemoveAt(0);
            }

            // 3. Lấy kho tương ứng ra để vẽ lên giao diện
            if (HistoryChartSeries.Count > 0)
            {
                var lineSeries = (LineSeries)HistoryChartSeries[0];

                double newValue = 0;
                if (SelectedMetric == "Temperature") newValue = temp;
                else if (SelectedMetric == "Humidity") newValue = hum;
                else if (SelectedMetric == "Smoke") newValue = smoke;

                // Bơm trực tiếp vào đầu mảng Values của LiveCharts
                lineSeries.Values.Add(newValue);
                ChartLabels.Add(currentTime);

                // Xóa điểm cũ để tạo hiệu ứng trôi ngang
                if (lineSeries.Values.Count > 15)
                {
                    lineSeries.Values.RemoveAt(0);
                    ChartLabels.RemoveAt(0);
                }
            }
        });
    }

    [RelayCommand] private async Task SelectMetric(string metric) { 
        SelectedMetric = metric;
        // Rút kho tương ứng ra vẽ
        if (metric == "Temperature") UpdateChartWithRealData(_timeHistory, _tempHistory);
        else if (metric == "Humidity") UpdateChartWithRealData(_timeHistory, _humHistory);
        else if (metric == "Smoke") UpdateChartWithRealData(_timeHistory, _smokeHistory);
    }

    [RelayCommand] private async Task SelectTimeFrame(string timeFrame) { 
        SelectedTimeFrame = timeFrame; 
        await LoadChartDataAsync(); 
    }

    public async Task LoadChartDataAsync()
    {
        UpdateChartWithRealData(new List<string>(), new List<double>());
    }

    // Tham số truyền vào là list labels (Trục X) và list values (Trục Y) lấy từ API
    private void UpdateChartWithRealData(List<string> labels, List<double> values)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (HistoryChartSeries.Count == 0)
            {
                HistoryChartSeries.Add(new LineSeries { PointGeometrySize = 10, LineSmoothness = 0.5 });
            }
            var lineSeries = (LineSeries)HistoryChartSeries[0];

            //HistoryChartSeries.Clear();
            ChartLabels.Clear();
            foreach (var label in labels) ChartLabels.Add(label);

            //var chartValues = new ChartValues<double>;
            string title = "";
            string colorHex = "#2196F3";

            if (SelectedMetric == "Temperature") { title = "Nhiệt độ (°C)"; colorHex = "#F44336"; }
            else if (SelectedMetric == "Humidity") { title = "Độ ẩm (%)"; colorHex = "#4CAF50"; }
            else if (SelectedMetric == "Smoke") { title = "Khói (ppm)"; colorHex = "#9E9E9E"; }

            // Cập nhật thuộc tính đồ thị
            lineSeries.Title = title;
            lineSeries.Stroke = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(colorHex);
            lineSeries.Fill = System.Windows.Media.Brushes.Transparent;

            // Nạp mảng giá trị (Ghi đè hoàn toàn mảng cũ bằng mảng mới từ Kho)
            lineSeries.Values = new ChartValues<double>(values);
        });
    }

    [RelayCommand]
    private async Task SendSupportRequest()
    {
        // Tương lai: ID căn hộ sẽ lấy từ lúc Đăng nhập. 
        // Hiện tại: Gắn cứng mã "P102" để test cho khớp với giao diện của Manager
        int apartmentId = 2;
        string alertMessage = "Chủ hộ đang gửi yêu cầu hỗ trợ khẩn cấp từ ứng dụng!";

        // Bắn tín hiệu qua ống SignalR lên Server
        if (_signalRService != null)
        {
            await _signalRService.SendSupportRequestToServerAsync(apartmentId, alertMessage);
            MessageBox.Show("Tín hiệu SOS đã được phát đi thành công!", "Đã gửi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}