using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Manager
{
    public class PheDuyetBaoCaoModel : PageModel
    {
        private readonly IConfiguration _config;
        public PheDuyetBaoCaoModel(IConfiguration config) { _config = config; }

        [TempData] public string SuccessMsg { get; set; }
        [TempData] public string ErrorMsg { get; set; }

        public class BaoCaoInfo
        {
            public string MaBaoCao { get; set; }
            public string NguoiLap { get; set; }
            public DateTime ThoiGianLap { get; set; }
            public string NoiDung { get; set; }
            public string TrangThai { get; set; }
            public string NguoiDuyet { get; set; }
        }

        public List<BaoCaoInfo> DanhSachBaoCao { get; set; } = new List<BaoCaoInfo>();

        public void OnGet() { LoadData(); }

        public IActionResult OnPostDuyet(string MaBaoCaoDuyet)
        {
            string maQuanLy = User.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            if (string.IsNullOrEmpty(maQuanLy))
            {
                ErrorMsg = "Lỗi bảo mật: Không nhận diện được mã Quản lý. Vui lòng đăng nhập lại!";
                LoadData(); return Page();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_07_BaoCao.sp_24_PheDuyetBaoCao", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaBaoCao", MaBaoCaoDuyet);
                        cmd.Parameters.AddWithValue("@MaNhanVienDuyet", maQuanLy);
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Đã phê duyệt thành công báo cáo mã: {MaBaoCaoDuyet}";
                    }
                }
                return RedirectToPage();
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }
            LoadData(); return Page();
        }

        private void LoadData()
        {
            DanhSachBaoCao.Clear();
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                string sql = @"SELECT B.MaBaoCao, B.ThoiGianLap, B.NoiDung, B.TrangThai, 
                                      N1.HoTen AS NguoiLap, N2.HoTen AS NguoiDuyet
                               FROM pkg_07_BaoCao.BAOCAO B
                               LEFT JOIN pkg_02_NhanSu.NHANVIEN N1 ON B.MaNhanVienLap = N1.MaNhanVien
                               LEFT JOIN pkg_02_NhanSu.NHANVIEN N2 ON B.MaNhanVienDuyet = N2.MaNhanVien
                               ORDER BY CASE WHEN B.TrangThai = N'Chờ phê duyệt' THEN 1 ELSE 2 END, B.ThoiGianLap DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DanhSachBaoCao.Add(new BaoCaoInfo
                        {
                            MaBaoCao = reader["MaBaoCao"].ToString(),
                            NguoiLap = reader["NguoiLap"] != DBNull.Value ? reader["NguoiLap"].ToString() : "N/A",
                            ThoiGianLap = Convert.ToDateTime(reader["ThoiGianLap"]),
                            NoiDung = reader["NoiDung"].ToString(),
                            TrangThai = reader["TrangThai"].ToString(),
                            NguoiDuyet = reader["NguoiDuyet"] != DBNull.Value ? reader["NguoiDuyet"].ToString() : ""
                        });
                    }
                }
            }
        }
    }
}