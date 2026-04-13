using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class ThemKhachHangModel : PageModel
    {
        private readonly IConfiguration _config;
        public ThemKhachHangModel(IConfiguration config) { _config = config; }

        [BindProperty] public string HoTen { get; set; }
        [BindProperty] public string CCCD { get; set; }
        [BindProperty] public string DienThoai { get; set; }
        [BindProperty] public string DiaChi { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public DateTime NgaySinh { get; set; } = DateTime.Today.AddYears(-20);

        public string SuccessMsg { get; set; }
        public string ErrorMsg { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_01_KhachHang.sp_02_ThemKhachHang", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@HoTen", HoTen);
                        cmd.Parameters.AddWithValue("@CCCD", CCCD);
                        cmd.Parameters.AddWithValue("@DienThoai", DienThoai);
                        cmd.Parameters.AddWithValue("@DiaChi", DiaChi ?? "");
                        cmd.Parameters.AddWithValue("@Email", Email ?? "");
                        cmd.Parameters.AddWithValue("@NgaySinh", NgaySinh);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string maKHMoi = reader["MaKhachHangMoi"].ToString();
                                SuccessMsg = $"Thêm khách hàng thành công! Mã hệ thống cấp: {maKHMoi}";
                                ModelState.Clear();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ErrorMsg = ex.Message;
            }
            return Page();
        }
    }
}