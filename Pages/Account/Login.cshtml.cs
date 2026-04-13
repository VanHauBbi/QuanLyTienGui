using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace QuanLyTienGui.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _config;
        public LoginModel(IConfiguration config) { _config = config; }

        [BindProperty] public string TenDangNhap { get; set; }
        [BindProperty] public string MatKhau { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("A")) return RedirectToPage("/Admin/Index");
                if (User.IsInRole("C")) return RedirectToPage("/Manager/ThongKeTongHop");
                return RedirectToPage("/Staff/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_03_TaiKhoan.sp_01_DangNhap", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TenDangNhap", TenDangNhap);
                        cmd.Parameters.AddWithValue("@MatKhau", MatKhau);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string maTK = reader["MaTaiKhoan"].ToString();
                                string username = reader["TenDangNhap"].ToString();
                                string vaiTro = reader["VaiTro"].ToString();
                                string maNV = reader["MaNhanVien"] != DBNull.Value ? reader["MaNhanVien"].ToString() : "";

                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, username),
                                    new Claim("MaTaiKhoan", maTK),
                                    new Claim("MaNhanVien", maNV),
                                    new Claim(ClaimTypes.Role, vaiTro)
                                };

                                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                                await HttpContext.SignInAsync("MyCookieAuth", principal);

                                if (vaiTro == "A") return RedirectToPage("/Admin/Index");
                                else if (vaiTro == "C") return RedirectToPage("/Manager/ThongKeTongHop");
                                else return RedirectToPage("/Staff/Index");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ErrorMessage = ex.Message;
            }

            return Page();
        }
    }
}