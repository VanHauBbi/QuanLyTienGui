using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string TenDangNhap { get; set; }

        [BindProperty]
        public string MatKhau { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Account/Login");
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("QuanLyTienGuiDB");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_DangNhap", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TenDangNhap", TenDangNhap);
                        cmd.Parameters.AddWithValue("@MatKhau", MatKhau); // Thực tế nên băm MD5/SHA256

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string vaiTro = reader["VaiTro"].ToString();

                                // Điều hướng tùy theo Role (A: Admin, B: Staff, C: Manager)
                                if (vaiTro == "A") return RedirectToPage("/Admin/Index");
                                else if (vaiTro == "C") return RedirectToPage("/Manager/Index");
                                else return RedirectToPage("/Staff/Index");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Bắt lỗi RAISERROR từ sp_DangNhap trong SQL Server
                ErrorMessage = ex.Message;
            }

            return Page();
        }
    }
}