using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyTienGui.Pages.Staff
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        public IndexModel(IConfiguration config) { _config = config; }

        public int TongKhachHang { get; set; } = 0;
        public int SoMoHomNay { get; set; } = 0;
        public int SoMoThangNay { get; set; } = 0;

        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartData { get; set; } = new List<int>();

        public void OnGet()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM pkg_01_KhachHang.KHACHHANG", conn))
                        TongKhachHang = (int)cmd.ExecuteScalar();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM pkg_06_GiaoDich.vw_05_GiaoDich_HomNay WHERE LoaiGiaoDich = N'Mở sổ'", conn))
                        SoMoHomNay = (int)cmd.ExecuteScalar();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM pkg_06_GiaoDich.vw_18_GiaoDich_MoSo WHERE MONTH(NgayGiaoDich) = MONTH(GETDATE()) AND YEAR(NgayGiaoDich) = YEAR(GETDATE())", conn))
                        SoMoThangNay = (int)cmd.ExecuteScalar();

                    DateTime startDate = DateTime.Today.AddDays(-6);
                    for (int i = 0; i < 7; i++)
                    {
                        ChartLabels.Add(startDate.AddDays(i).ToString("dd/MM"));
                        ChartData.Add(0);
                    }

                    string sqlChart = @"SELECT CAST(NgayGiaoDich AS DATE) as Ngay, COUNT(*) as SoLuong
                                        FROM pkg_06_GiaoDich.vw_18_GiaoDich_MoSo
                                        WHERE NgayGiaoDich >= @StartDate
                                        GROUP BY CAST(NgayGiaoDich AS DATE)";

                    using (SqlCommand cmdChart = new SqlCommand(sqlChart, conn))
                    {
                        cmdChart.Parameters.AddWithValue("@StartDate", startDate);
                        using (SqlDataReader reader = cmdChart.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime ngay = Convert.ToDateTime(reader["Ngay"]);
                                int count = Convert.ToInt32(reader["SoLuong"]);
                                string label = ngay.ToString("dd/MM");
                                int index = ChartLabels.IndexOf(label);
                                if (index >= 0) ChartData[index] = count;
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
    }
}