using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Account
{
    public class QuenMatKhauModel : PageModel
    {
        private readonly IConfiguration _config;
        public QuenMatKhauModel(IConfiguration config) { _config = config; }

        [BindProperty] public string TenDangNhap { get; set; }
        [BindProperty] public string Email { get; set; }

        public string ErrorMsg { get; set; }
        public string SuccessMsg { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    string checkSql = @"SELECT t.MaTaiKhoan FROM pkg_03_TaiKhoan.TAIKHOAN t 
                                JOIN pkg_02_NhanSu.NHANVIEN n ON t.MaTaiKhoan = n.MaTaiKhoan 
                                WHERE t.TenDangNhap = @TenDN AND n.Email = @Email AND t.TrangThai = 1";

                    string maTK = null;
                    using (SqlCommand cmdCheck = new SqlCommand(checkSql, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@TenDN", TenDangNhap);
                        cmdCheck.Parameters.AddWithValue("@Email", Email);
                        var result = cmdCheck.ExecuteScalar();
                        if (result != null) maTK = result.ToString();
                    }

                    if (string.IsNullOrEmpty(maTK))
                    {
                        ErrorMsg = "Tên đăng nhập hoặc Email không chính xác hoặc tài khoản đã bị khóa!";
                        return Page();
                    }

                    string updateSql = "UPDATE pkg_03_TaiKhoan.TAIKHOAN SET MatKhau = '1' WHERE MaTaiKhoan = @MaTK";
                    using (SqlCommand cmdUpdate = new SqlCommand(updateSql, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@MaTK", maTK);
                        cmdUpdate.ExecuteNonQuery();
                        SuccessMsg = "Mật khẩu đã được khôi phục về mặc định: 1. Vui lòng đăng nhập và đổi mật khẩu ngay!";
                    }
                }
            }
            catch (Exception ex) { ErrorMsg = "Lỗi hệ thống: " + ex.Message; }

            return Page();
        }
    }
}