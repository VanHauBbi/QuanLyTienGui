using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Manager
{
    public class BaoCaoDoanhSoModel : PageModel
    {
        private readonly IConfiguration _config;
        public BaoCaoDoanhSoModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty] public int Thang { get; set; } = DateTime.Now.Month;
        [BindProperty] public int Nam { get; set; } = DateTime.Now.Year;

        public string ErrorMsg { get; set; }

        public decimal TongThuThang { get; set; } = 0;
        public decimal TongChiThang { get; set; } = 0;

        public class DoanhSoNgay
        {
            public int Ngay { get; set; }
            public decimal TongThu { get; set; }
            public decimal TongChi { get; set; }
        }

        public List<DoanhSoNgay> DanhSachDoanhSo { get; set; } = new List<DoanhSoNgay>();

        public List<int> ChartLabels { get; set; } = new List<int>();
        public List<decimal> ChartThu { get; set; } = new List<decimal>();
        public List<decimal> ChartChi { get; set; } = new List<decimal>();

        public void OnGet()
        {
            LoadBaoCao();
        }

        public IActionResult OnPost()
        {
            LoadBaoCao();
            return Page();
        }

        private void LoadBaoCao()
        {
            DanhSachDoanhSo.Clear();
            ChartLabels.Clear();
            ChartThu.Clear();
            ChartChi.Clear();
            TongThuThang = 0;
            TongChiThang = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_09_ThongKe.sp_23_XemBaoCaoDoanhSo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Thang", Thang);
                        cmd.Parameters.AddWithValue("@Nam", Nam);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int ngay = Convert.ToInt32(reader["Ngay"]);
                                decimal thu = Convert.ToDecimal(reader["TongThu"]);
                                decimal chi = Convert.ToDecimal(reader["TongChi"]);

                                DanhSachDoanhSo.Add(new DoanhSoNgay { Ngay = ngay, TongThu = thu, TongChi = chi });

                                ChartLabels.Add(ngay);
                                ChartThu.Add(thu);
                                ChartChi.Add(chi);

                                TongThuThang += thu;
                                TongChiThang += chi;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
            }
        }
    }
}