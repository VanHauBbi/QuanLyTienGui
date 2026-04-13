using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Admin
{
    public class QuanLyTaiKhoanModel : PageModel
    {
        private readonly IConfiguration _config;
        public QuanLyTaiKhoanModel(IConfiguration config) { _config = config; }

        [BindProperty] public string ActionType { get; set; } = "CapPhat";
        [BindProperty] public string MaTaiKhoan { get; set; }
        [BindProperty] public string MaNhanVienDuocCap { get; set; }
        [BindProperty] public string TenDangNhap { get; set; }
        [BindProperty] public string VaiTroMoi { get; set; }

        [TempData] public string SuccessMsg { get; set; }
        [TempData] public string ErrorMsg { get; set; }

        public class TaiKhoanInfo
        {
            public string MaTK { get; set; }
            public string Username { get; set; }
            public string VaiTro { get; set; }
            public string NguoiSoHuu { get; set; }
        }

        public class NhanVienInfo
        {
            public string MaNV { get; set; }
            public string HoTen { get; set; }
            public string ChucVu { get; set; }
        }

        public List<TaiKhoanInfo> DanhSachTK { get; set; } = new List<TaiKhoanInfo>();
        public List<NhanVienInfo> DanhSachNVChuaCoTK { get; set; } = new List<NhanVienInfo>();

        public void OnGet() { LoadData(); }

        public IActionResult OnPostLuuTaiKhoan()
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

                        if (ActionType == "CapPhat")
                        {
                            cmd.CommandText = "pkg_03_TaiKhoan.sp_27_CapPhatTaiKhoanChoNhanVien";
                            cmd.Parameters.AddWithValue("@MaNhanVien", MaNhanVienDuocCap);
                            cmd.Parameters.AddWithValue("@TenDangNhap", TenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhau", "1");
                            cmd.ExecuteNonQuery();
                            SuccessMsg = $"Đã cấp phát tài khoản {TenDangNhap} cho nhân viên. Pass mặc định: 1.";
                        }
                        else if (ActionType == "PhanQuyen")
                        {
                            cmd.CommandText = "pkg_15_QuyenHan.sp_31_PhanQuyenHeThong";
                            cmd.Parameters.AddWithValue("@MaTaiKhoan", MaTaiKhoan);
                            cmd.Parameters.AddWithValue("@VaiTroMoi", VaiTroMoi);
                            cmd.ExecuteNonQuery();
                            SuccessMsg = $"Đã phân quyền thành công. Tài khoản {TenDangNhap} hiện giữ vai trò: {(VaiTroMoi == "A" ? "ADMIN" : VaiTroMoi == "C" ? "MANAGER" : "STAFF")}.";
                        }
                        else if (ActionType == "ThemDocLap")
                        {
                            cmd.CommandText = "pkg_03_TaiKhoan.sp_26_ThemTaiKhoan";
                            cmd.Parameters.AddWithValue("@TenDangNhap", TenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhau", "1");
                            cmd.Parameters.AddWithValue("@VaiTro", VaiTroMoi);
                            cmd.ExecuteNonQuery();
                            SuccessMsg = $"Đã tạo tài khoản độc lập {TenDangNhap}. Pass mặc định: 1.";
                        }
                    }
                }
                return RedirectToPage(); // Chống lỗi F5
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 50000)
                    ErrorMsg = "Lỗi: Tên đăng nhập này đã có người sử dụng!";
                else
                    ErrorMsg = ex.Message;
            }
            LoadData();
            return Page();
        }

        public IActionResult OnPostXoaTaiKhoan(string MaTKXoa)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_03_TaiKhoan.sp_29_XoaTaiKhoan", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", MaTKXoa);
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Đã xóa thành công tài khoản: {MaTKXoa}";
                    }
                }
                return RedirectToPage();
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }
            LoadData(); return Page();
        }

        public IActionResult OnPostResetPass(string MaTKReset)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_03_TaiKhoan.sp_28_SuaTaiKhoan", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", MaTKReset);
                        cmd.Parameters.AddWithValue("@MatKhau", "1");
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Đã khôi phục mật khẩu của tài khoản {MaTKReset} về mặc định (1).";
                    }
                }
                return RedirectToPage();
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }
            LoadData(); return Page();
        }

        private void LoadData()
        {
            DanhSachTK.Clear();
            DanhSachNVChuaCoTK.Clear();
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                using (SqlCommand cmdNV = new SqlCommand("SELECT MaNhanVien, HoTen, ChucVu FROM pkg_02_NhanSu.NHANVIEN WHERE MaTaiKhoan IS NULL", conn))
                using (SqlDataReader readerNV = cmdNV.ExecuteReader())
                {
                    while (readerNV.Read())
                    {
                        DanhSachNVChuaCoTK.Add(new NhanVienInfo
                        {
                            MaNV = readerNV["MaNhanVien"].ToString(),
                            HoTen = readerNV["HoTen"].ToString(),
                            ChucVu = readerNV["ChucVu"].ToString()
                        });
                    }
                }

                string sqlTK = @"SELECT T.MaTaiKhoan, T.TenDangNhap, T.VaiTro, N.HoTen 
                                 FROM pkg_03_TaiKhoan.TAIKHOAN T 
                                 LEFT JOIN pkg_02_NhanSu.NHANVIEN N ON T.MaTaiKhoan = N.MaTaiKhoan
                                 WHERE T.TrangThai = 1";
                using (SqlCommand cmdTK = new SqlCommand(sqlTK, conn))
                using (SqlDataReader readerTK = cmdTK.ExecuteReader())
                {
                    while (readerTK.Read())
                    {
                        DanhSachTK.Add(new TaiKhoanInfo
                        {
                            MaTK = readerTK["MaTaiKhoan"].ToString(),
                            Username = readerTK["TenDangNhap"].ToString(),
                            VaiTro = readerTK["VaiTro"].ToString(),
                            NguoiSoHuu = readerTK["HoTen"] != DBNull.Value ? readerTK["HoTen"].ToString() : "Tài khoản Độc lập"
                        });
                    }
                }
            }
        }
    }
}