using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class RutTienModel : PageModel
    {
        private readonly IConfiguration _config;
        public RutTienModel(IConfiguration config) { _config = config; }

        [BindProperty(SupportsGet = true)] public string MaSoTietKiem { get; set; }
        public string MaNhanVien { get; set; }

        public string TenKhachHang { get; set; }
        public string TenLoaiTietKiem { get; set; }
        public decimal SoDuHienTai { get; set; }
        public DateTime NgayMoSo { get; set; }
        public DateTime NgayDaoHan { get; set; }
        public string TrangThai { get; set; }

        public int SoNgayDaGui { get; set; }
        public decimal LaiSuatApDung { get; set; }
        public decimal TienLaiDuTinh { get; set; }
        public decimal TongTienNhan { get; set; }
        public bool ChoPhepRut { get; set; } = false;
        public string ThongBaoNghiepVu { get; set; }

        [TempData] public string ErrorMessage { get; set; }
        [TempData] public string SuccessMessage { get; set; }

        public void OnGet() { if (!string.IsNullOrEmpty(MaSoTietKiem)) LoadThongTinSo(MaSoTietKiem); }

        public IActionResult OnPostTruyVan() { return RedirectToPage(new { MaSoTietKiem = this.MaSoTietKiem }); }

        public IActionResult OnPostTatToan()
        {
            MaNhanVien = User.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            if (string.IsNullOrEmpty(MaNhanVien))
            {
                ErrorMessage = "Lỗi bảo mật: Không nhận diện được nhân viên. Vui lòng đăng nhập lại!";
                return RedirectToPage(new { MaSoTietKiem = this.MaSoTietKiem });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_06_GiaoDich.sp_09_TatToanSo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaSoTietKiem", MaSoTietKiem);
                        cmd.Parameters.AddWithValue("@MaNhanVien", MaNhanVien);
                        cmd.ExecuteNonQuery();

                        SuccessMessage = $"Giao dịch thành công! Đã tất toán toàn bộ tiền cho sổ {MaSoTietKiem}.";
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
                    if (endIndex != -1) ErrorMessage = rawMsg.Substring(startIndex, endIndex - startIndex + 1);
                    else ErrorMessage = rawMsg.Substring(startIndex);
                }
                else if (rawMsg.Contains("Cảnh báo"))
                {
                    int startIndex = rawMsg.IndexOf("Cảnh báo");
                    int endIndex = rawMsg.IndexOf("!", startIndex);
                    if (endIndex != -1) ErrorMessage = rawMsg.Substring(startIndex, endIndex - startIndex + 1);
                    else ErrorMessage = rawMsg.Substring(startIndex);
                }
                else
                {
                    ErrorMessage = "Lỗi hệ thống: " + ex.Message;
                }
            }
            return RedirectToPage(new { MaSoTietKiem = this.MaSoTietKiem });
        }

        private void LoadThongTinSo(string maSo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    string sql = @"
                        SELECT K.HoTen, L.TenLoai, S.SoTienGui, S.NgayMoSo, S.NgayDaoHan, S.TrangThai, L.KyHan,
                               ISNULL((SELECT TOP 1 PhanTramLai FROM pkg_05_LaiSuat.LAISUAT LS WHERE LS.MaLoaiTietKiem = S.MaLoaiTietKiem AND LS.NgayApDung <= S.NgayMoSo ORDER BY LS.NgayApDung DESC), 0) as PhanTramLai,
                               ISNULL((SELECT TOP 1 PhanTramLai FROM pkg_05_LaiSuat.LAISUAT LS JOIN pkg_04_TietKiem.LOAITIETKIEM LTK ON LS.MaLoaiTietKiem = LTK.MaLoaiTietKiem WHERE LTK.KyHan = 0 AND LS.NgayApDung <= GETDATE() ORDER BY LS.NgayApDung DESC), 0.5) as LaiKhongKyHan
                        FROM pkg_04_TietKiem.SOTIETKIEM S
                        JOIN pkg_01_KhachHang.KHACHHANG K ON S.MaKhachHang = K.MaKhachHang
                        JOIN pkg_04_TietKiem.LOAITIETKIEM L ON S.MaLoaiTietKiem = L.MaLoaiTietKiem
                        WHERE S.MaSoTietKiem = @MaSoTietKiem";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaSoTietKiem", maSo);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TenKhachHang = reader["HoTen"].ToString();
                                TenLoaiTietKiem = reader["TenLoai"].ToString();
                                SoDuHienTai = Convert.ToDecimal(reader["SoTienGui"]);
                                NgayMoSo = Convert.ToDateTime(reader["NgayMoSo"]);
                                NgayDaoHan = Convert.ToDateTime(reader["NgayDaoHan"]);
                                TrangThai = reader["TrangThai"].ToString();

                                int kyHan = Convert.ToInt32(reader["KyHan"]);
                                decimal phanTramLaiGoc = Convert.ToDecimal(reader["PhanTramLai"]);
                                decimal laiKhongKyHan = Convert.ToDecimal(reader["LaiKhongKyHan"]);

                                SoNgayDaGui = (DateTime.Today - NgayMoSo).Days;

                                if (TrangThai != "Đang hoạt động")
                                {
                                    ChoPhepRut = false;
                                    ThongBaoNghiepVu = "Sổ này đã tất toán hoặc đã bị khóa.";
                                    TienLaiDuTinh = 0;
                                    LaiSuatApDung = phanTramLaiGoc;
                                }
                                else if (SoNgayDaGui < 15)
                                {
                                    ChoPhepRut = false;
                                    ThongBaoNghiepVu = $"Sổ mới gửi được {SoNgayDaGui} ngày. (Quy định: tối thiểu 15 ngày mới được rút).";
                                    TienLaiDuTinh = 0;
                                    LaiSuatApDung = phanTramLaiGoc;
                                }
                                else
                                {
                                    ChoPhepRut = true;
                                    if (kyHan == 0)
                                    {
                                        LaiSuatApDung = phanTramLaiGoc;
                                        TienLaiDuTinh = SoDuHienTai * (LaiSuatApDung / 100m) * SoNgayDaGui / 365m;
                                        ThongBaoNghiepVu = "Đủ điều kiện tất toán bình thường.";
                                    }
                                    else
                                    {
                                        int soNgayQuyDinh = kyHan * 30;
                                        if (SoNgayDaGui < soNgayQuyDinh)
                                        {
                                            LaiSuatApDung = laiKhongKyHan;
                                            TienLaiDuTinh = SoDuHienTai * (LaiSuatApDung / 100m) * SoNgayDaGui / 365m;
                                            ThongBaoNghiepVu = $"Rút trước hạn. Khách hàng chỉ được hưởng lãi suất Không kỳ hạn ({LaiSuatApDung}%).";
                                        }
                                        else
                                        {
                                            LaiSuatApDung = phanTramLaiGoc;
                                            TienLaiDuTinh = SoDuHienTai * (LaiSuatApDung / 100m) * soNgayQuyDinh / 365m;
                                            ThongBaoNghiepVu = "Đã đến hạn. Khách hàng được hưởng trọn vẹn tiền lãi.";
                                        }
                                    }
                                }

                                TienLaiDuTinh = Math.Round(TienLaiDuTinh, 0);
                                TongTienNhan = SoDuHienTai + TienLaiDuTinh;
                            }
                            else
                            {
                                ErrorMessage = "Không tìm thấy Sổ tiết kiệm này trong hệ thống!";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorMessage = "Lỗi hệ thống: " + ex.Message; }
        }
    }
}