using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        public IndexModel(IConfiguration config) { _config = config; }

        public int TongTaiKhoan { get; set; } = 0;
        public int SoTaiKhoanAdmin { get; set; } = 0;
        public int SoTaiKhoanManager { get; set; } = 0;
        public int SoTaiKhoanStaff { get; set; } = 0;

        public string ErrorMsg { get; set; }

        public void OnGet()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM pkg_03_TaiKhoan.TAIKHOAN", conn))
                    {
                        TongTaiKhoan = (int)cmd.ExecuteScalar();
                    }
                    string sqlRole = @"SELECT VaiTro, COUNT(*) as SoLuong FROM pkg_03_TaiKhoan.TAIKHOAN GROUP BY VaiTro";
                    using (SqlCommand cmd = new SqlCommand(sqlRole, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string role = reader["VaiTro"].ToString();
                            int count = Convert.ToInt32(reader["SoLuong"]);

                            if (role == "A") SoTaiKhoanAdmin = count;
                            else if (role == "C") SoTaiKhoanManager = count;
                            else if (role == "B") SoTaiKhoanStaff = count;
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorMsg = "Lỗi kết nối CSDL: " + ex.Message; }
        }
    }
}