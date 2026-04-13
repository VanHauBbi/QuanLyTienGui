using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Staff
{
    public class CapNhatKhachHangModel : PageModel
    {
        private readonly IConfiguration _config;
        public CapNhatKhachHangModel(IConfiguration config) { _config = config; }

        [BindProperty(SupportsGet = true)] public string MaKhachHang { get; set; }
        [BindProperty] public string HoTen { get; set; }
        [BindProperty] public string CCCD { get; set; }
        [BindProperty] public string DienThoai { get; set; }
        [BindProperty] public string DiaChi { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public DateTime NgaySinh { get; set; }

        public string SuccessMsg { get; set; }
        public string ErrorMsg { get; set; }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(MaKhachHang))
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM pkg_01_KhachHang.KHACHHANG WHERE MaKhachHang = @MaKH", conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", MaKhachHang);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                HoTen = reader["HoTen"].ToString();
                                CCCD = reader["CCCD"].ToString();
                                DienThoai = reader["DienThoai"].ToString();
                                DiaChi = reader["DiaChi"].ToString();
                                Email = reader["Email"].ToString();
                                NgaySinh = reader["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(reader["NgaySinh"]) : DateTime.Today.AddYears(-18);
                            }
                        }
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_01_KhachHang.sp_03_CapNhatKhachHang", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKhachHang", MaKhachHang);
                        cmd.Parameters.AddWithValue("@HoTen", HoTen);
                        cmd.Parameters.AddWithValue("@DienThoai", DienThoai);
                        cmd.Parameters.AddWithValue("@DiaChi", DiaChi ?? "");
                        cmd.Parameters.AddWithValue("@Email", Email ?? "");
                        cmd.Parameters.AddWithValue("@NgaySinh", NgaySinh);

                        cmd.ExecuteNonQuery();
                        SuccessMsg = "Cập nhật hồ sơ khách hàng thành công!";
                    }
                }
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }

            return Page();
        }
    }
}