using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class TraCuuSoModel : PageModel
    {
        private readonly IConfiguration _config;
        public TraCuuSoModel(IConfiguration config) { _config = config; }

        [BindProperty] public string TuKhoa { get; set; }

        public class ThongTinSo
        {
            public string MaSo { get; set; }
            public string TenKhachHang { get; set; }
            public string TenLoai { get; set; }
            public decimal SoDu { get; set; }
            public DateTime NgayMo { get; set; }
            public DateTime NgayDaoHan { get; set; }
            public string TrangThai { get; set; }
        }

        public string SuccessMsg { get; set; }
        public string ErrorMsg { get; set; }

        public List<ThongTinSo> DanhSachSo { get; set; } = new List<ThongTinSo>();

        public void OnGet() { LoadDuLieu(""); }

        public IActionResult OnPost() { LoadDuLieu(TuKhoa); return Page(); }

        private void LoadDuLieu(string tuKhoa)
        {
            DanhSachSo.Clear();
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                string sql = @"
                    SELECT S.MaSoTietKiem, K.HoTen, L.TenLoai, S.SoTienGui, S.NgayMoSo, S.NgayDaoHan, S.TrangThai 
                    FROM pkg_04_TietKiem.SOTIETKIEM S 
                    JOIN pkg_01_KhachHang.KHACHHANG K ON S.MaKhachHang = K.MaKhachHang
                    JOIN pkg_04_TietKiem.LOAITIETKIEM L ON S.MaLoaiTietKiem = L.MaLoaiTietKiem
                    WHERE (@TuKhoa = '' OR S.MaSoTietKiem LIKE '%' + @TuKhoa + '%' OR K.HoTen LIKE N'%' + @TuKhoa + '%')
                    ORDER BY S.NgayMoSo DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TuKhoa", tuKhoa ?? "");
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DanhSachSo.Add(new ThongTinSo
                            {
                                MaSo = reader["MaSoTietKiem"].ToString(),
                                TenKhachHang = reader["HoTen"].ToString(),
                                TenLoai = reader["TenLoai"].ToString(),
                                SoDu = Convert.ToDecimal(reader["SoTienGui"]),
                                NgayMo = Convert.ToDateTime(reader["NgayMoSo"]),
                                NgayDaoHan = Convert.ToDateTime(reader["NgayDaoHan"]),
                                TrangThai = reader["TrangThai"].ToString()
                            });
                        }
                    }
                }
            }
        }

        public IActionResult OnPostTaiTucSo(string MaSoTaiTuc)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_04_TietKiem.sp_10_TaiTucSo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaSoTietKiem", MaSoTaiTuc);
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Thành công! Hệ thống đã kết toán lãi và tự động gia hạn kỳ hạn mới cho sổ {MaSoTaiTuc}.";
                    }
                }
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }

            LoadDuLieu("");
            return Page();
        }
    }
}