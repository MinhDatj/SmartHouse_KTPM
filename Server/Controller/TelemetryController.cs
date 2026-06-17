using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Collections.Generic;

namespace SmartBuilding.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly SqlProvider2 _dbProvider;

        public TelemetryController()
        {
            _dbProvider = new SqlProvider2();
        }

        [HttpGet("history/{sensorId}")]
        public IActionResult GetSensorHistory(int sensorId, [FromQuery] string range)
        {
            DateTime fromDate = DateTime.UtcNow;

            switch (range.ToLower())
            {
                case "1h": fromDate = fromDate.AddHours(-1); break;
                case "24h": fromDate = fromDate.AddDays(-1); break;
                case "7d": fromDate = fromDate.AddDays(-7); break;
                default: return BadRequest("Khoảng thời gian không hợp lệ (1h, 24h, 7d).");
            }

            string sql = $"SELECT GiaTri, ThoiGian FROM LichSuDo";
            string where = $"ID_CamBien = {sensorId} AND ThoiGian >= '{fromDate:yyyy-MM-dd HH:mm:ss}'";
            string order = "ThoiGian ASC";

            DataTable dt = _dbProvider.Select(sql, where, order);

            var result = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add(new
                {
                    GiaTri = Convert.ToDouble(row["GiaTri"]),
                    ThoiGian = Convert.ToDateTime(row["ThoiGian"])
                });
            }

            return Ok(result);
        }
    }
}