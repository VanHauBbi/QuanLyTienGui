using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class LichSuGiaoDichModel : PageModel
    {
        private readonly IConfiguration _config;
        public LichSuGiaoDichModel(IConfiguration config) { _config = config; }

        public class GiaoDichInfo
        {
            public string MaGD { get; set; }
            public string MaSo { get; set; }
            public string LoaiGiaoDich { get; set; }
            public decimal SoTien { get; set; }
            public DateTime NgayGD { get; set; }
            public string NhanVienThucHien { get; set; }
        }

        public List<GiaoDichInfo> DanhSachGiaoDich { get; set; } = new List<GiaoDichInfo>();

        public void OnGet()
        {
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                string sql = @"SELECT TOP 50 MaGiaoDich, MaSoTietKiem, LoaiGiaoDich, SoTien, NgayGiaoDich, MaNhanVien 
                               FROM pkg_06_GiaoDich.GIAODICH ORDER BY NgayGiaoDich DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DanhSachGiaoDich.Add(new GiaoDichInfo
                        {
                            MaGD = reader["MaGiaoDich"].ToString(),
                            MaSo = reader["MaSoTietKiem"].ToString(),
                            LoaiGiaoDich = reader["LoaiGiaoDich"].ToString(),
                            SoTien = Convert.ToDecimal(reader["SoTien"]),
                            NgayGD = Convert.ToDateTime(reader["NgayGiaoDich"]),
                            NhanVienThucHien = reader["MaNhanVien"].ToString()
                        });
                    }
                }
            }
        }
    }
}