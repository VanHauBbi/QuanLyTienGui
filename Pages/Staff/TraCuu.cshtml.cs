using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class TraCuuModel : PageModel
    {
        private readonly IConfiguration _config;
        public TraCuuModel(IConfiguration config) { _config = config; }

        [BindProperty] public string TuKhoa { get; set; }

        public class KhachHangInfo
        {
            public string MaKH { get; set; }
            public string HoTen { get; set; }
            public string CCCD { get; set; }
            public string DienThoai { get; set; }
            public string Email { get; set; }
        }

        public string ErrorMsg { get; set; }
        public string SuccessMsg { get; set; }

        public List<KhachHangInfo> KhachHangs { get; set; } = new List<KhachHangInfo>();

        public void OnGet()
        {
            LoadDuLieu("");
        }

        public IActionResult OnPost()
        {
            LoadDuLieu(TuKhoa);
            return Page();
        }

        private void LoadDuLieu(string tuKhoa)
        {
            KhachHangs.Clear();
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                SqlCommand cmd;

                if (string.IsNullOrEmpty(tuKhoa))
                {
                    cmd = new SqlCommand("SELECT * FROM pkg_01_KhachHang.KHACHHANG ORDER BY MaKhachHang DESC", conn);
                }
                else
                {
                    cmd = new SqlCommand("pkg_01_KhachHang.sp_05_TraCuuKhachHang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TuKhoa", tuKhoa);
                }

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        KhachHangs.Add(new KhachHangInfo
                        {
                            MaKH = reader["MaKhachHang"].ToString(),
                            HoTen = reader["HoTen"].ToString(),
                            CCCD = reader["CCCD"].ToString(),
                            DienThoai = reader["DienThoai"].ToString(),
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : ""
                        });
                    }
                }
            }
        }

        public IActionResult OnPostXoaKhachHang(string MaKhachHangXoa)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_01_KhachHang.sp_04_XoaKhachHang", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKhachHang", MaKhachHangXoa);
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Đã xóa thành công hồ sơ khách hàng {MaKhachHangXoa}!";
                    }
                }
            }
            catch (SqlException ex)
            {
                ErrorMsg = "Cảnh báo hệ thống: " + ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMsg = "Lỗi hệ thống: " + ex.Message;
            }

            LoadDuLieu(TuKhoa ?? "");
            return Page();
        }
    }
}