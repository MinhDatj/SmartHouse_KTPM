using System;

namespace Server.Models; // Sử dụng File-scoped namespace giống core của nhóm

public class AlertPayload
{
    // Dùng 'required' cho các trường bắt buộc phải có khi báo động
    public required string ApartmentId { get; set; }
    
    public required string AlertType { get; set; } // Ví dụ: "Temperature" hoặc "Smoke"
    
    public double CurrentValue { get; set; }
    
    // Dùng 'string?' cho trường có thể rỗng giống phong cách của Tech Lead
    public string? Message { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public required string Status { get; set; } = "DANGER";
}