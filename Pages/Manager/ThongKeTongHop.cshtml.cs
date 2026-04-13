using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Manager
{
    public class ThongKeTongHopModel : PageModel
    {
        private readonly IConfiguration _config;
        public ThongKeTongHopModel(IConfiguration config)
        {
            _config = config;
        }

        public int TongKhachHang { get; set; } = 0;
        public int TongSoDangMo { get; set; } = 0;
        public decimal TongHuyDongVon { get; set; } = 0;
        public string ErrorMsg { get; set; }

        public void OnGet()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_09_ThongKe.sp_25_ThongKeTongHop", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TongKhachHang = reader["TongKhachHang"] != DBNull.Value ? Convert.ToInt32(reader["TongKhachHang"]) : 0;
                                TongSoDangMo = reader["TongSoDangMo"] != DBNull.Value ? Convert.ToInt32(reader["TongSoDangMo"]) : 0;
                                TongHuyDongVon = reader["TongHuyDongVon"] != DBNull.Value ? Convert.ToDecimal(reader["TongHuyDongVon"]) : 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "Không thể tải dữ liệu thống kê: " + ex.Message;
            }
        }
    }
}