using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class MoSoTietKiemModel : PageModel
    {
        private readonly IConfiguration _config;
        public MoSoTietKiemModel(IConfiguration config) { _config = config; }

        [BindProperty(SupportsGet = true)] public string MaKhachHang { get; set; }
        [BindProperty] public string MaLoaiTietKiem { get; set; }
        [BindProperty] public decimal SoTienGui { get; set; }

        public string MaNhanVien { get; set; }
        public string TenKhachHang { get; set; }

        [TempData] public string MaSoVuaTao { get; set; }
        [TempData] public string ErrorMsg { get; set; }
        [TempData] public string SuccessMsg { get; set; }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(MaKhachHang))
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT pkg_01_KhachHang.fn_11_GetTenKH(@MaKH)", conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", MaKhachHang);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value) TenKhachHang = result.ToString();
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            MaNhanVien = User.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            if (string.IsNullOrEmpty(MaNhanVien))
            {
                ErrorMsg = "Lỗi bảo mật: Không nhận diện được nhân viên. Vui lòng đăng nhập lại!";
                return RedirectToPage(new { MaKhachHang = this.MaKhachHang });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_06_GiaoDich.sp_06_MoSoTietKiem", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKhachHang", MaKhachHang);
                        cmd.Parameters.AddWithValue("@MaLoaiTietKiem", MaLoaiTietKiem);
                        cmd.Parameters.AddWithValue("@SoTienGui", SoTienGui);
                        cmd.Parameters.AddWithValue("@MaNhanVien", MaNhanVien);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                MaSoVuaTao = reader["MaSoMoi"].ToString();
                                SuccessMsg = $"Giao dịch hoàn tất! Hệ thống đã tự động sinh Sổ Tiết Kiệm mã: {MaSoVuaTao}";
                            }
                        }
                    }
                }
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }

            return RedirectToPage(new { MaKhachHang = this.MaKhachHang });
        }
    }
}