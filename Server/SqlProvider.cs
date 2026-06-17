using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SmartBuilding.Server
{
    public class SqlProvider2
    {
        private readonly string _conn;

        public SqlProvider2()
        {
            // Đường dẫn kết nối trực tiếp đến file Database .mdf của bạn -> Đổi lại đường dẫn đến file database trên máy khi chạy
            _conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SmartBuildingDB;Integrated Security=True;Trust Server Certificate=True";
        }

        // Hàm truy vấn đọc dữ liệu (Select)
        public DataTable Select(string sql, string where = null, string order = null)
        {
            if (where != null) sql += " WHERE " + where;
            if (order != null) sql += " ORDER BY " + order;

            var dt = new DataTable();
            using (var conn = new SqlConnection(_conn))
            {
                conn.Open();
                var cmd = new SqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        // Hàm thực thi các lệnh ghi dữ liệu (Insert, Update, Delete)
        public int ExecuteNonQuery(string sql, SqlParameter[]? parameters = null)
        {
            using (var conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // Hàm kiểm tra Đăng nhập an toàn (Chống SQL Injection)
        public DataTable CheckLogin(string user, string password)
        {
            string sql = "SELECT * FROM NguoiDung WHERE Account = @User AND Password = @Password";
            var dt = new DataTable();

            using (var conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@User", user);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }
    }
}