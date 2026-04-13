using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Manager
{
    public class QuanLyNhanVienModel : PageModel
    {
        private readonly IConfiguration _config;
        public QuanLyNhanVienModel(IConfiguration config) { _config = config; }

        [BindProperty] public string MaNhanVien { get; set; }
        [BindProperty] public string HoTen { get; set; }
        [BindProperty] public string CCCD { get; set; }
        [BindProperty] public string DienThoai { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string DiaChi { get; set; }
        [BindProperty] public DateTime NgaySinh { get; set; } = DateTime.Today.AddYears(-20);

        [TempData] public string SuccessMsg { get; set; }
        [TempData] public string ErrorMsg { get; set; }

        public class NhanVienInfo
        {
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string CCCD { get; set; }
            public string DienThoai { get; set; }
            public string Email { get; set; }
            public string DiaChi { get; set; }
            public DateTime? NgaySinh { get; set; }
            public string ChucVu { get; set; }
            public string Username { get; set; }
        }

        public List<NhanVienInfo> DanhSachNV { get; set; } = new List<NhanVienInfo>();

        public void OnGet() { LoadData(); }

        public IActionResult OnPostLuuNhanVien()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        string tempSuccess = "";

                        if (string.IsNullOrEmpty(MaNhanVien))
                        {
                            cmd.CommandText = "pkg_02_NhanSu.sp_19_ThemNhanVien";
                            tempSuccess = $"Đã tiếp nhận hồ sơ nhân viên mới: {HoTen}.";
                        }
                        else
                        {
                            cmd.CommandText = "pkg_02_NhanSu.sp_20_CapNhatNhanVien";
                            cmd.Parameters.AddWithValue("@MaNhanVien", MaNhanVien);
                            tempSuccess = $"Đã cập nhật hồ sơ cho nhân viên: {MaNhanVien}.";
                        }

                        cmd.Parameters.AddWithValue("@HoTen", HoTen);
                        cmd.Parameters.AddWithValue("@CCCD", CCCD);
                        cmd.Parameters.AddWithValue("@DienThoai", DienThoai);
                        cmd.Parameters.AddWithValue("@Email", Email ?? "");
                        cmd.Parameters.AddWithValue("@DiaChi", DiaChi ?? "");
                        cmd.Parameters.AddWithValue("@NgaySinh", NgaySinh);

                        cmd.ExecuteNonQuery();

                        SuccessMsg = tempSuccess;
                    }
                }
                return RedirectToPage();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    if (ex.Message.Contains("CCCD"))
                        ErrorMsg = "Lỗi: Số CCCD này đã tồn tại trong hệ thống!";
                    else if (ex.Message.Contains("Email"))
                        ErrorMsg = "Lỗi: Địa chỉ Email này đã được sử dụng!";
                    else if (ex.Message.Contains("DienThoai"))
                        ErrorMsg = "Lỗi: Số điện thoại này đã tồn tại!";
                    else
                        ErrorMsg = "Lỗi: Dữ liệu bị trùng lặp (CCCD/Email/SĐT đã tồn tại)!";
                }
                else
                {
                    string rawMsg = ex.Message;
                    if (rawMsg.Contains("Lỗi: "))
                    {
                        int startIndex = rawMsg.IndexOf("Lỗi: ");
                        int endIndex = rawMsg.IndexOf("!", startIndex);
                        if (endIndex == -1) endIndex = rawMsg.IndexOf(".", startIndex);
                        if (endIndex != -1) ErrorMsg = rawMsg.Substring(startIndex, endIndex - startIndex + 1);
                        else ErrorMsg = rawMsg.Substring(startIndex);
                    }
                    else
                    {
                        ErrorMsg = "Lỗi hệ thống: " + ex.Message;
                    }
                }
            }

            LoadData();
            return Page();
        }

        public IActionResult OnPostXoaNhanVien(string MaNVXoa)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_02_NhanSu.sp_21_XoaNhanVien", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaNhanVien", MaNVXoa);
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Đã xóa hồ sơ nhân viên: {MaNVXoa}";
                    }
                }
                return RedirectToPage();
            }
            catch (SqlException ex)
            {
                string rawMsg = ex.Message;
                if (rawMsg.Contains("Lỗi: "))
                {
                    int startIndex = rawMsg.IndexOf("Lỗi: ");
                    int endIndex = rawMsg.IndexOf("!", startIndex);
                    if (endIndex == -1) endIndex = rawMsg.IndexOf(".", startIndex);
                    if (endIndex != -1) ErrorMsg = rawMsg.Substring(startIndex, endIndex - startIndex + 1);
                    else ErrorMsg = rawMsg.Substring(startIndex);
                }
                else ErrorMsg = "Lỗi: Không thể xóa vì nhân viên này đang có dữ liệu liên kết!";
            }

            LoadData();
            return Page();
        }

        private void LoadData()
        {
            DanhSachNV.Clear();
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pkg_02_NhanSu.sp_22_XemDanhSachNhanVien", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DanhSachNV.Add(new NhanVienInfo
                            {
                                MaNV = reader["MaNhanVien"].ToString(),
                                HoTen = reader["HoTen"].ToString(),
                                CCCD = reader["CCCD"].ToString(),
                                DienThoai = reader["DienThoai"].ToString(),
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "",
                                DiaChi = reader["DiaChi"] != DBNull.Value ? reader["DiaChi"].ToString() : "",
                                NgaySinh = reader["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(reader["NgaySinh"]) : (DateTime?)null,
                                ChucVu = reader["ChucVu"].ToString(),
                                Username = reader["TenDangNhap"] != DBNull.Value ? reader["TenDangNhap"].ToString() : ""
                            });
                        }
                    }
                }
            }
        }
    }
}