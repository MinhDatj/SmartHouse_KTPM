using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartBuilding.Server;
using SmartBuilding.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Khai báo các dịch vụ hệ thống Web API, SignalR, Swagger
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kích hoạt tiến trình chạy ngầm Worker hứng MQTT
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SensorHub>("/sensorHub");

app.Run();