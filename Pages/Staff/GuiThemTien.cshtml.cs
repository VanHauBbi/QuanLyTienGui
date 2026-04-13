using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Staff
{
    public class GuiThemTienModel : PageModel
    {
        private readonly IConfiguration _config;
        public GuiThemTienModel(IConfiguration config) { _config = config; }

        [BindProperty(SupportsGet = true)] public string MaSoTietKiem { get; set; }
        [BindProperty] public decimal SoTienNhan { get; set; }
        public string MaNhanVien { get; set; }

        // CÁC BIẾN LƯU THÔNG TIN HIỂN THỊ
        public string TenKhachHang { get; set; }
        public string CCCD { get; set; }
        public string TenLoai { get; set; }
        public int KyHan { get; set; }
        public decimal SoDuHienTai { get; set; }
        public DateTime NgayMoSo { get; set; }
        public string TrangThai { get; set; }

        [TempData] public string SuccessMsg { get; set; }
        [TempData] public string ErrorMsg { get; set; }

        public void OnGet() { if (!string.IsNullOrEmpty(MaSoTietKiem)) LoadThongTin(); }

        public IActionResult OnPostTruyVan() { return RedirectToPage(new { MaSoTietKiem = this.MaSoTietKiem }); }

        public IActionResult OnPostNapTien()
        {
            MaNhanVien = User.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            if (string.IsNullOrEmpty(MaNhanVien))
            {
                ErrorMsg = "Lỗi bảo mật: Không nhận diện được nhân viên. Vui lòng đăng nhập lại!";
                return RedirectToPage(new { MaSoTietKiem = this.MaSoTietKiem });
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_06_GiaoDich.sp_08_GuiThemTien", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaSoTietKiem", MaSoTietKiem);
                        cmd.Parameters.AddWithValue("@SoTienGuiThem", SoTienNhan);
                        cmd.Parameters.AddWithValue("@MaNhanVien", MaNhanVien);
                        cmd.ExecuteNonQuery();

                        SuccessMsg = $"Giao dịch nạp tiền thành công! Đã nạp {SoTienNhan.ToString("N0")} VNĐ vào sổ {MaSoTietKiem}.";
                    }
                }
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
                else ErrorMsg = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToPage(new { MaSoTietKiem = this.MaSoTietKiem });
        }

        private void LoadThongTin()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    string sql = @"
                        SELECT K.HoTen, K.CCCD, L.TenLoai, L.KyHan, S.SoTienGui, S.NgayMoSo, S.TrangThai
                        FROM pkg_04_TietKiem.SOTIETKIEM S
                        JOIN pkg_01_KhachHang.KHACHHANG K ON S.MaKhachHang = K.MaKhachHang
                        JOIN pkg_04_TietKiem.LOAITIETKIEM L ON S.MaLoaiTietKiem = L.MaLoaiTietKiem
                        WHERE S.MaSoTietKiem = @MaSoTietKiem";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaSoTietKiem", MaSoTietKiem);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TenKhachHang = reader["HoTen"].ToString();
                                CCCD = reader["CCCD"].ToString();
                                TenLoai = reader["TenLoai"].ToString();
                                KyHan = Convert.ToInt32(reader["KyHan"]);
                                SoDuHienTai = Convert.ToDecimal(reader["SoTienGui"]);
                                NgayMoSo = Convert.ToDateTime(reader["NgayMoSo"]);
                                TrangThai = reader["TrangThai"].ToString();
                            }
                            else ErrorMsg = "Mã sổ tiết kiệm không tồn tại!";
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorMsg = "Lỗi: " + ex.Message; }
        }
    }
}