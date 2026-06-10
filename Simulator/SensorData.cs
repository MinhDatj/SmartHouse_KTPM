using System;

namespace Simulator 
{
    public class SensorData
    {
        public string CanHoId { get; set; }
        public double NhietDo { get; set; }
        public bool Khoi { get; set; }
        public bool CheDoVangNha { get; set; }
        public bool CuaMo { get; set; }
        public double BuiMinPM25 { get; set; }
        public bool SanNhaUotBan { get; set; }
        
        // Thêm trường thời gian tại đây
        public DateTime ThoiGian { get; set; } 
    }
}