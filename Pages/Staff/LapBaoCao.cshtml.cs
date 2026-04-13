using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Staff
{
    public class LapBaoCaoModel : PageModel
    {
        private readonly IConfiguration _config;
        public LapBaoCaoModel(IConfiguration config) { _config = config; }

        [BindProperty] public string NoiDung { get; set; }

        [TempData] public string SuccessMsg { get; set; }
        [TempData] public string ErrorMsg { get; set; }

        // Bổ sung các biến lưu số liệu tự động
        public int TongGiaoDich { get; set; } = 0;
        public decimal TongThu { get; set; } = 0;
        public decimal TongChi { get; set; } = 0;

        public class BaoCaoCuaToi
        {
            public string MaBaoCao { get; set; }
            public DateTime ThoiGianLap { get; set; }
            public string NoiDung { get; set; }
            public string TrangThai { get; set; }
            public string NguoiDuyet { get; set; }
        }

        public List<BaoCaoCuaToi> DanhSachBaoCao { get; set; } = new List<BaoCaoCuaToi>();

        public void OnGet()
        {
            LoadData();
            GenerateReportTemplate();
        }

        public IActionResult OnPost()
        {
            string maNhanVien = User.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            if (string.IsNullOrEmpty(maNhanVien))
            {
                ErrorMsg = "Lỗi bảo mật: Không nhận diện được tài khoản. Vui lòng đăng nhập lại!";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(NoiDung))
            {
                ErrorMsg = "Vui lòng nhập nội dung báo cáo!";
                return RedirectToPage();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    string sqlInsert = @"
                        DECLARE @Count INT;
                        SELECT @Count = COUNT(*) + 1 FROM pkg_07_BaoCao.BAOCAO WITH (UPDLOCK);
                        DECLARE @MaBC VARCHAR(10) = 'BC' + RIGHT('00000' + CAST(@Count AS VARCHAR(5)), 5);
                        
                        INSERT INTO pkg_07_BaoCao.BAOCAO (MaBaoCao, MaNhanVienLap, ThoiGianLap, NoiDung, TrangThai)
                        VALUES (@MaBC, @MaNhanVien, GETDATE(), @NoiDung, N'Chờ phê duyệt');
                    ";

                    using (SqlCommand cmd = new SqlCommand(sqlInsert, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
                        cmd.Parameters.AddWithValue("@NoiDung", NoiDung);
                        cmd.ExecuteNonQuery();

                        SuccessMsg = "Đã gửi báo cáo chốt ca thành công! Vui lòng chờ Quản lý phê duyệt.";
                    }
                }
            }
            catch (SqlException ex) { ErrorMsg = "Lỗi hệ thống: " + ex.Message; }

            return RedirectToPage();
        }

        private void GenerateReportTemplate()
        {
            string maNhanVien = User.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            if (string.IsNullOrEmpty(maNhanVien)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    // Tự động gom số liệu giao dịch trong ngày của nhân viên này
                    string sqlStats = @"
                        SELECT 
                            COUNT(MaGiaoDich) AS TongGD,
                            ISNULL(SUM(CASE WHEN LoaiGiaoDich IN (N'Mở sổ', N'Gửi thêm') THEN SoTien ELSE 0 END), 0) AS TongThu,
                            ISNULL(SUM(CASE WHEN LoaiGiaoDich IN (N'Tất toán', N'Rút tiền') THEN SoTien ELSE 0 END), 0) AS TongChi
                        FROM pkg_06_GiaoDich.GIAODICH
                        WHERE MaNhanVien = @MaNhanVien AND CAST(NgayGiaoDich AS DATE) = CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(sqlStats, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TongGiaoDich = Convert.ToInt32(reader["TongGD"]);
                                TongThu = Convert.ToDecimal(reader["TongThu"]);
                                TongChi = Convert.ToDecimal(reader["TongChi"]);
                            }
                        }
                    }
                }

                // Điền sẵn số liệu vào form để nhân viên đỡ phải tự gõ
                if (string.IsNullOrEmpty(NoiDung))
                {
                    NoiDung = $"--- BÁO CÁO KẾT QUẢ GIAO DỊCH NGÀY {DateTime.Now.ToString("dd/MM/yyyy")} ---\n" +
                              $"- Tổng số lượt giao dịch: {TongGiaoDich} lượt\n" +
                              $"- TỔNG TIỀN THU (Khách nộp): {TongThu.ToString("N0")} VNĐ\n" +
                              $"- TỔNG TIỀN CHI (Trút cho khách): {TongChi.ToString("N0")} VNĐ\n" +
                              $"- Số dư cuối ngày thực tế tại quầy: (Khớp / Lệch ...)\n" +
                              $"- Ghi chú khác: Mọi thứ bình thường.";
                }
            }
            catch (Exception) { /* Bỏ qua nếu lỗi */ }
        }

        private void LoadData()
        {
            string maNhanVien = User.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            if (string.IsNullOrEmpty(maNhanVien)) return;

            DanhSachBaoCao.Clear();
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                string sql = @"
                    SELECT B.MaBaoCao, B.ThoiGianLap, B.NoiDung, B.TrangThai, 
                           ND.HoTen AS NguoiDuyet
                    FROM pkg_07_BaoCao.BAOCAO B
                    LEFT JOIN pkg_02_NhanSu.NHANVIEN ND ON B.MaNhanVienDuyet = ND.MaNhanVien
                    WHERE B.MaNhanVienLap = @MaNhanVien
                    ORDER BY B.ThoiGianLap DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DanhSachBaoCao.Add(new BaoCaoCuaToi
                            {
                                MaBaoCao = reader["MaBaoCao"].ToString(),
                                ThoiGianLap = Convert.ToDateTime(reader["ThoiGianLap"]),
                                NoiDung = reader["NoiDung"].ToString(),
                                TrangThai = reader["TrangThai"].ToString(),
                                NguoiDuyet = reader["NguoiDuyet"] != DBNull.Value ? reader["NguoiDuyet"].ToString() : "---"
                            });
                        }
                    }
                }
            }
        }
    }
}