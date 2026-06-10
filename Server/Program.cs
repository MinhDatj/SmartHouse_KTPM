using Server;
using Server.Hubs;
using Server.Services;

// 1. Khởi tạo Builder chuẩn Web thay vì Host
var builder = WebApplication.CreateBuilder(args);

// 2. Đăng ký dịch vụ SignalR 
builder.Services.AddSignalR();

// Đăng ký "bộ não" cảnh báo vào hệ thống DI Container
builder.Services.AddSingleton<AlertProcessingService>();

// 3. Đăng ký Service chạy ngầm gốc của dự án
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// 4. Mở cổng kết nối cho WPF Client
app.MapHub<ApartmentHub>("/apartmentHub");

app.Run();