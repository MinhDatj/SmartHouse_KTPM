using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

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

    // --- CÁC BIẾN CHO TAB THÔNG BÁO ---
    [ObservableProperty] private ObservableCollection<NotificationModel> notifications;

    public ResidentDashboardViewModel()
    {
        HistoryChartSeries = new SeriesCollection();
        ChartLabels = new ObservableCollection<string>();
        Notifications = new ObservableCollection<NotificationModel>();

        UpdateChart();
        InitializeMockNotifications(); // Khởi tạo dữ liệu thông báo giả lập
        StartMockDataSimulator();
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
        });
    }

    [RelayCommand] private void SelectMetric(string metric) { SelectedMetric = metric; UpdateChart(); }
    [RelayCommand] private void SelectTimeFrame(string timeFrame) { SelectedTimeFrame = timeFrame; UpdateChart(); }

    private void UpdateChart()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            HistoryChartSeries.Clear();
            ChartLabels.Clear();

            var chartValues = new ChartValues<double>();
            string title = "";
            string colorHex = "#2196F3";

            if (SelectedTimeFrame == "1h") { XAxisTitle = "Phút trước"; for (int i = 60; i >= 0; i -= 10) ChartLabels.Add(i.ToString()); }
            else if (SelectedTimeFrame == "24h") { XAxisTitle = "Giờ trước"; for (int i = 24; i >= 0; i -= 3) ChartLabels.Add(i.ToString()); }
            else if (SelectedTimeFrame == "7d") { XAxisTitle = "Ngày trước"; for (int i = 7; i >= 0; i--) ChartLabels.Add(i.ToString()); }

            int pointCount = ChartLabels.Count;
            Random rand = new Random();

            if (SelectedMetric == "Temperature") { title = "Nhiệt độ (°C)"; colorHex = "#F44336"; for (int i = 0; i < pointCount; i++) chartValues.Add(Math.Round(rand.NextDouble() * 10 + 25, 1)); }
            else if (SelectedMetric == "Humidity") { title = "Độ ẩm (%)"; colorHex = "#4CAF50"; for (int i = 0; i < pointCount; i++) chartValues.Add(Math.Round(rand.NextDouble() * 20 + 50, 1)); }
            else if (SelectedMetric == "Smoke") { title = "Khói (ppm)"; colorHex = "#9E9E9E"; for (int i = 0; i < pointCount; i++) chartValues.Add(Math.Round(rand.NextDouble() * 100 + 100, 1)); }

            HistoryChartSeries.Add(new LineSeries
            {
                Title = title,
                Values = chartValues,
                Stroke = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(colorHex),
                Fill = System.Windows.Media.Brushes.Transparent,
                PointGeometrySize = 10,
                LineSmoothness = 0.5
            });
        });
    }

    [RelayCommand]
    private void SendSupportRequest()
    {
        MessageBox.Show("Đã gửi yêu cầu hỗ trợ đến Ban Quản Lý!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}