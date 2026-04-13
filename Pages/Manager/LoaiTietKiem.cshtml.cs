using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace QuanLyTienGui.Pages.Manager
{
    public class LoaiTietKiemModel : PageModel
    {
        private readonly IConfiguration _config;
        public LoaiTietKiemModel(IConfiguration config) { _config = config; }

        [BindProperty] public string MaLoaiTietKiem { get; set; }
        [BindProperty] public string TenLoai { get; set; }
        [BindProperty] public int KyHan { get; set; }
        [BindProperty] public decimal SoTienToiThieu { get; set; }
        [BindProperty] public string HinhThucTaiTuc { get; set; }
        [BindProperty] public string HinhThucTraLai { get; set; }
        [BindProperty] public string MoTa { get; set; }
        [BindProperty] public bool TrangThaiApDung { get; set; } = true;

        [TempData] public string SuccessMsg { get; set; }
        [TempData] public string ErrorMsg { get; set; }

        public class LoaiTietKiemInfo
        {
            public string MaLoai { get; set; }
            public string TenLoai { get; set; }
            public int KyHan { get; set; }
            public decimal SoTienToiThieu { get; set; }
            public string HinhThucTaiTuc { get; set; }
            public string HinhThucTraLai { get; set; }
            public string MoTa { get; set; }
            public bool TrangThaiApDung { get; set; }
        }

        public List<LoaiTietKiemInfo> DanhSachLoai { get; set; } = new List<LoaiTietKiemInfo>();

        public void OnGet() { LoadData(); }

        public IActionResult OnPostLuuLoai()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(MaLoaiTietKiem))
                    {
                        cmd = new SqlCommand("pkg_04_TietKiem.sp_13_ThemLoaiTietKiem", conn);
                        SuccessMsg = $"Đã thêm mới loại tiết kiệm: {TenLoai}!";
                    }
                    else
                    {
                        cmd = new SqlCommand("pkg_04_TietKiem.sp_14_CapNhatLoaiTietKiem", conn);
                        cmd.Parameters.AddWithValue("@MaLoaiTietKiem", MaLoaiTietKiem);
                        SuccessMsg = $"Đã cập nhật loại tiết kiệm: {TenLoai}!";
                    }

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TenLoai", TenLoai);
                    cmd.Parameters.AddWithValue("@KyHan", KyHan);
                    cmd.Parameters.AddWithValue("@SoTienToiThieu", SoTienToiThieu);
                    cmd.Parameters.AddWithValue("@HinhThucTaiTuc", HinhThucTaiTuc);
                    cmd.Parameters.AddWithValue("@HinhThucTraLai", HinhThucTraLai);
                    cmd.Parameters.AddWithValue("@MoTa", MoTa ?? "");
                    cmd.Parameters.AddWithValue("@TrangThaiApDung", TrangThaiApDung);

                    cmd.ExecuteNonQuery();
                }
                return RedirectToPage();
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }
            LoadData(); return Page();
        }

        public IActionResult OnPostXoaLoai(string MaLoaiXoa)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_04_TietKiem.sp_15_XoaLoaiTietKiem", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaLoaiTietKiem", MaLoaiXoa);
                        cmd.ExecuteNonQuery();
                        SuccessMsg = $"Đã ngừng áp dụng gói tiết kiệm: {MaLoaiXoa}";
                    }
                }
                return RedirectToPage();
            }
            catch (SqlException ex) { ErrorMsg = ex.Message; }
            LoadData(); return Page();
        }

        private void LoadData()
        {
            DanhSachLoai.Clear();
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM pkg_04_TietKiem.vw_14_LoaiTietKiem_DangApDung", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DanhSachLoai.Add(new LoaiTietKiemInfo
                        {
                            MaLoai = reader["MaLoaiTietKiem"].ToString(),
                            TenLoai = reader["TenLoai"].ToString(),
                            KyHan = Convert.ToInt32(reader["KyHan"]),
                            SoTienToiThieu = Convert.ToDecimal(reader["SoTienToiThieu"]),
                            HinhThucTaiTuc = reader["HinhThucTaiTuc"].ToString(),
                            HinhThucTraLai = reader["HinhThucTraLai"].ToString(),
                            MoTa = reader["MoTa"].ToString(),
                            TrangThaiApDung = Convert.ToBoolean(reader["TrangThaiApDung"])
                        });
                    }
                }
            }
        }
    }
}