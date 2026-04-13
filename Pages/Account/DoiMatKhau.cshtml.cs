using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Account
{
    public class DoiMatKhauModel : PageModel
    {
        private readonly IConfiguration _config;
        public DoiMatKhauModel(IConfiguration config) { _config = config; }

        [BindProperty] public string TenDangNhap { get; set; }
        [BindProperty] public string MatKhauCu { get; set; }
        [BindProperty] public string MatKhauMoi { get; set; }
        [BindProperty] public string XacNhanMatKhau { get; set; }

        public string ErrorMsg { get; set; }
        public string SuccessMsg { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (MatKhauMoi != XacNhanMatKhau)
            {
                ErrorMsg = "Mật khẩu xác nhận không khớp!";
                return Page();
            }

            if (MatKhauMoi == MatKhauCu)
            {
                ErrorMsg = "Mật khẩu mới phải khác mật khẩu hiện tại!";
                return Page();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();

                    string checkSql = @"SELECT MaTaiKhoan FROM pkg_03_TaiKhoan.TAIKHOAN 
                                        WHERE TenDangNhap = @TenDN AND MatKhau = @MkCu AND TrangThai = 1";

                    string maTK = null;
                    using (SqlCommand cmdCheck = new SqlCommand(checkSql, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@TenDN", TenDangNhap);
                        cmdCheck.Parameters.AddWithValue("@MkCu", MatKhauCu);
                        var result = cmdCheck.ExecuteScalar();
                        if (result != null) maTK = result.ToString();
                    }

                    if (string.IsNullOrEmpty(maTK))
                    {
                        ErrorMsg = "Tên đăng nhập hoặc mật khẩu cũ không chính xác!";
                        return Page();
                    }

                    string updateSql = "UPDATE pkg_03_TaiKhoan.TAIKHOAN SET MatKhau = @MkMoi WHERE MaTaiKhoan = @MaTK";
                    using (SqlCommand cmdUpdate = new SqlCommand(updateSql, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@MkMoi", MatKhauMoi);
                        cmdUpdate.Parameters.AddWithValue("@MaTK", maTK);
                        cmdUpdate.ExecuteNonQuery();

                        SuccessMsg = "Chúc mừng! Bạn đã đổi mật khẩu thành công.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "Lỗi hệ thống: " + ex.Message;
            }

            return Page();
        }
    }
}