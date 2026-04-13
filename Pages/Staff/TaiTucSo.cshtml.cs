using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class TaiTucSoModel : PageModel
    {
        private readonly IConfiguration _config;
        public TaiTucSoModel(IConfiguration config) { _config = config; }

        [BindProperty(SupportsGet = true)] public string MaSoTietKiem { get; set; }

        public string TenKhachHang { get; set; }
        public string TenLoaiTietKiem { get; set; }
        public decimal SoDuHienTai { get; set; }
        public DateTime NgayMoSo { get; set; }
        public DateTime NgayDaoHan { get; set; }
        public string TrangThai { get; set; }
        public int KyHan { get; set; }

        public decimal LaiSuat { get; set; }
        public decimal TienLaiDuTinh { get; set; }
        public decimal SoDuMoi { get; set; }
        public DateTime NgayDaoHanMoi { get; set; }
        public bool ChoPhepTaiTuc { get; set; } = false;
        public string ThongBaoNghiepVu { get; set; }

        [TempData] public string SuccessMessage { get; set; }
        [TempData] public string ErrorMessage { get; set; }

        public void OnGet() { if (!string.IsNullOrEmpty(MaSoTietKiem)) LoadThongTinSo(MaSoTietKiem); }

        public IActionResult OnPostTruyVan() { return RedirectToPage(new { MaSoTietKiem = this.MaSoTietKiem }); }

        public IActionResult OnPostXacNhanTaiTuc()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_04_TietKiem.sp_10_TaiTucSo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaSoTietKiem", MaSoTietKiem);
                        cmd.ExecuteNonQuery();

                        SuccessMessage = $"Thành công! Sổ {MaSoTietKiem} đã được chốt lãi và gia hạn sang kỳ mới.";
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
                else ErrorMessage = "Lỗi hệ thống: " + ex.Message;
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
                        SELECT K.HoTen, L.TenLoai, L.KyHan, S.SoTienGui, S.NgayMoSo, S.NgayDaoHan, S.TrangThai,
                               ISNULL((SELECT TOP 1 PhanTramLai FROM pkg_05_LaiSuat.LAISUAT LS WHERE LS.MaLoaiTietKiem = S.MaLoaiTietKiem AND LS.NgayApDung <= S.NgayMoSo ORDER BY LS.NgayApDung DESC), 0) as PhanTramLai
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
                                KyHan = Convert.ToInt32(reader["KyHan"]);
                                SoDuHienTai = Convert.ToDecimal(reader["SoTienGui"]);
                                NgayMoSo = Convert.ToDateTime(reader["NgayMoSo"]);
                                NgayDaoHan = Convert.ToDateTime(reader["NgayDaoHan"]);
                                TrangThai = reader["TrangThai"].ToString();
                                LaiSuat = Convert.ToDecimal(reader["PhanTramLai"]);

                                if (KyHan > 0)
                                {
                                    int soNgayQuyDinh = (NgayDaoHan.Date - NgayMoSo.Date).Days;
                                    if (soNgayQuyDinh <= 0) soNgayQuyDinh = KyHan * 30;
                                    TienLaiDuTinh = Math.Round(SoDuHienTai * (LaiSuat / 100m) * soNgayQuyDinh / 365m, 0);
                                }
                                else
                                {
                                    int soNgayDaGui = (DateTime.Today - NgayMoSo.Date).Days;
                                    TienLaiDuTinh = Math.Round(SoDuHienTai * (LaiSuat / 100m) * soNgayDaGui / 365m, 0);
                                }

                                if (TrangThai != "Đang hoạt động")
                                {
                                    ChoPhepTaiTuc = false;
                                    ThongBaoNghiepVu = "Sổ này đã tất toán hoặc bị khóa, không thể tái tục.";
                                    TienLaiDuTinh = 0;
                                }
                                else if (KyHan == 0)
                                {
                                    ChoPhepTaiTuc = false;
                                    ThongBaoNghiepVu = "Sản phẩm Không kỳ hạn không sử dụng chức năng tái tục thủ công.";
                                }
                                else if (DateTime.Today < NgayDaoHan)
                                {
                                    ChoPhepTaiTuc = false;
                                    ThongBaoNghiepVu = $"Chưa đến ngày đáo hạn ({NgayDaoHan.ToString("dd/MM/yyyy")}). Không thể tái tục sớm.";
                                }
                                else
                                {
                                    ChoPhepTaiTuc = true;
                                    ThongBaoNghiepVu = "Sổ đã đến hạn. Hệ thống sẽ kết toán lãi và cộng dồn vào gốc (Ngoại trừ trường hợp khách chọn Lãi nhập tài khoản).";

                                    SoDuMoi = SoDuHienTai + TienLaiDuTinh;
                                    NgayDaoHanMoi = NgayDaoHan.AddMonths(KyHan);
                                }
                            }
                            else ErrorMessage = "Không tìm thấy Sổ tiết kiệm này!";
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorMessage = "Lỗi hệ thống: " + ex.Message; }
        }
    }
}