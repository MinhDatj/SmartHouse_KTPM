using Microsoft.EntityFrameworkCore;
using SmartBuilding.Domain.Entities; // Sử dụng các Entity từ project Core

namespace SmartBuilding.Data;

public class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    // Constructor này bắt buộc phải có để nhận Connection String từ Backend Server truyền vào
    public DbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    // Khai báo các bảng sẽ xuất hiện trong Database của bạn
    public DbSet<NguoiDung> NguoiDungs { get; set; }
    public DbSet<CanHo> CanHos { get; set; }
    public DbSet<Phong> Phongs { get; set; }
    public DbSet<CamBien> CamBiens { get; set; }
    public DbSet<LichSuDo> LichSuDos { get; set; }
    public DbSet<QuanLyToaNha> QuanLyToaNhas { get; set; }
    public DbSet<NhanVien> NhanViens { get; set; }
    public DbSet<YeuCauHoTro> YeuCauHoTros { get; set; }
    public DbSet<ThietBi> ThietBis {  get; set; }

    // Nơi cấu hình nâng cao (Tối ưu hiệu năng Database)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kỹ thuật nâng cao: Tạo Index hỗn hợp cho bảng lịch sử đo.
        // Vì bảng này lưu dữ liệu mỗi 5 giây, việc tạo Index theo ID_CamBien và ThoiGian
        // Sẽ giúp màn hình biểu đồ WPF (LiveCharts) truy vấn cực nhanh khi vẽ đồ thị.
        modelBuilder.Entity<LichSuDo>()
            .HasIndex(l => new { l.ID_CamBien, l.ThoiGian });

        // Bạn có thể cấu hình thêm các ràng buộc khác ở đây nếu cần
    }
}