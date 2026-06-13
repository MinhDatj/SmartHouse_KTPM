using System;

namespace Simulator;

public class PhongKhach
{
    public bool CuaChinhMo { get; set; }
    public bool TiviBat { get; set; }
    public bool DieuHoaBat { get; set; }
    public bool QuatBat { get; set; }
    public bool DenBat { get; set; }
    public double NhietDo { get; set; }
}

public class PhongBep
{
    public bool BepTuBat { get; set; }
    public bool MayHutMuiBat { get; set; }
    public bool DenBat { get; set; }
    public double NhietDo { get; set; }
    public bool PhatHienKhoi { get; set; }
}

public class PhongNgu
{
    public bool DieuHoaBat { get; set; }
    public bool DenBat { get; set; }
    public double NhietDo { get; set; }
}

public class PhongTam
{
    public bool DenBat { get; set; }
    public bool DenSuoiBat { get; set; }
    public bool BinhNongLanhBat { get; set; }
    public double NhietDo { get; set; }
}

public class SensorData
{ 
    public DateTime ThoiGian { get; set; }
    
    // Gom các phòng vào
    public PhongKhach Khach { get; set; }
    public PhongBep Bep { get; set; }
    public PhongNgu Ngu { get; set; }
    public PhongTam Tam { get; set; }
}