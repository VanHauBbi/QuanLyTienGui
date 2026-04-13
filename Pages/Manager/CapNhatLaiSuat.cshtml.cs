using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Manager
{
    public class CapNhatLaiSuatModel : PageModel
    {
        private readonly IConfiguration _config;
        public CapNhatLaiSuatModel(IConfiguration config) { _config = config; }

        [BindProperty] public string MaLaiSuat { get; set; }
        [BindProperty] public string MaLoaiTietKiem { get; set; }
        [BindProperty] public decimal PhanTramLai { get; set; }
        [BindProperty] public DateTime NgayApDung { get; set; } = DateTime.Today;

        [TempData] public string SuccessMsg { get; set; }
        [TempData] public string ErrorMsg { get; set; }

        public class LaiSuatInfo
        {
            public string MaLaiSuat { get; set; }
            public string TenLoaiTietKiem { get; set; }
            public decimal PhanTramLai { get; set; }
            public DateTime NgayApDung { get; set; }
            public string MaLoaiTietKiem { get; set; }
        }

        public class LoaiTietKiemOption
        {
            public string MaLoai { get; set; }
            public string TenLoai { get; set; }
        }

        public List<LaiSuatInfo> DanhSachLaiSuat { get; set; } = new List<LaiSuatInfo>();
        public List<LoaiTietKiemOption> DanhSachLoai { get; set; } = new List<LoaiTietKiemOption>();

        public void OnGet() { LoadData(); }

        public IActionResult OnPostLuuLaiSuat()
        {
            if (NgayApDung.Date < DateTime.Today)
            {
                ErrorMsg = "Lỗi: Ngày áp dụng không được chọn vào thời điểm trong quá khứ!";
                LoadData();
                return Page();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (string.IsNullOrEmpty(MaLaiSuat))
                        {
                            cmd.CommandText = "pkg_05_LaiSuat.sp_16_ThemBieuLaiSuat";
                            cmd.Parameters.AddWithValue("@MaLoaiTietKiem", MaLoaiTietKiem);
                            cmd.Parameters.AddWithValue("@PhanTramLai", PhanTramLai);
                            cmd.Parameters.AddWithValue("@NgayApDung", NgayApDung);
                            cmd.ExecuteNonQuery();
                            SuccessMsg = "Đã ban hành Biểu Lãi Suất mới thành công!";
                        }
                        else
                        {
                            cmd.CommandText = "pkg_05_LaiSuat.sp_17_CapNhatBieuLaiSuat";
                            cmd.Parameters.AddWithValue("@MaLaiSuat", MaLaiSuat);
                            cmd.Parameters.AddWithValue("@PhanTramLai", PhanTramLai);
                            cmd.Parameters.AddWithValue("@NgayApDung", NgayApDung);
                            cmd.ExecuteNonQuery();
                            SuccessMsg = $"Đã cập nhật lại mức lãi suất cho mã {MaLaiSuat}!";
                        }
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
                else if (rawMsg.Contains("Cảnh báo"))
                {
                    int startIndex = rawMsg.IndexOf("Cảnh báo");
                    int endIndex = rawMsg.IndexOf("!", startIndex);
                    if (endIndex != -1) ErrorMsg = rawMsg.Substring(startIndex, endIndex - startIndex + 1);
                    else ErrorMsg = rawMsg.Substring(startIndex);
                }
                else
                {
                    ErrorMsg = "Lỗi hệ thống: " + ex.Message;
                }
            }
            LoadData(); return Page();
        }

        public IActionResult OnPostXoaLaiSuat(string MaLaiSuatXoa)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_05_LaiSuat.sp_18_XoaBieuLaiSuat", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaLaiSuat", MaLaiSuatXoa);
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Đã hủy bỏ mức lãi suất mã {MaLaiSuatXoa}!";
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
                else ErrorMsg = "Lỗi hệ thống: Không thể xóa vì ràng buộc dữ liệu!";
            }
            LoadData(); return Page();
        }

        private void LoadData()
        {
            DanhSachLaiSuat.Clear();
            DanhSachLoai.Clear();

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                using (SqlCommand cmdLoai = new SqlCommand("SELECT MaLoaiTietKiem, TenLoai FROM pkg_04_TietKiem.vw_14_LoaiTietKiem_DangApDung", conn))
                using (SqlDataReader readerLoai = cmdLoai.ExecuteReader())
                {
                    while (readerLoai.Read())
                    {
                        DanhSachLoai.Add(new LoaiTietKiemOption
                        {
                            MaLoai = readerLoai["MaLoaiTietKiem"].ToString(),
                            TenLoai = readerLoai["TenLoai"].ToString()
                        });
                    }
                }

                string sqlLaiSuat = @"SELECT LS.MaLaiSuat, LTK.TenLoai, LS.PhanTramLai, LS.NgayApDung, LS.MaLoaiTietKiem 
                                      FROM pkg_05_LaiSuat.LAISUAT LS
                                      JOIN pkg_04_TietKiem.vw_14_LoaiTietKiem_DangApDung LTK ON LS.MaLoaiTietKiem = LTK.MaLoaiTietKiem
                                      ORDER BY LS.NgayApDung DESC";

                using (SqlCommand cmdLS = new SqlCommand(sqlLaiSuat, conn))
                using (SqlDataReader readerLS = cmdLS.ExecuteReader())
                {
                    while (readerLS.Read())
                    {
                        DanhSachLaiSuat.Add(new LaiSuatInfo
                        {
                            MaLaiSuat = readerLS["MaLaiSuat"].ToString(),
                            TenLoaiTietKiem = readerLS["TenLoai"].ToString(),
                            PhanTramLai = Convert.ToDecimal(readerLS["PhanTramLai"]),
                            NgayApDung = Convert.ToDateTime(readerLS["NgayApDung"]),
                            MaLoaiTietKiem = readerLS["MaLoaiTietKiem"].ToString()
                        });
                    }
                }
            }
        }
    }
}