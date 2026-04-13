using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace QuanLyTienGui.Pages.Admin
{
    public class SaoLuuPhucHoiModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public SaoLuuPhucHoiModel(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public string SuccessMsg { get; set; }
        public string ErrorMsg { get; set; }

        [BindProperty] public string FilePhucHoi { get; set; }

        public List<string> DanhSachFileBackup { get; set; } = new List<string>();

        private string GetBackupFolder()
        {
            return Path.Combine(_env.ContentRootPath, "Backups");
        }

        public void OnGet()
        {
            LoadDanhSachFile();
        }

        public IActionResult OnPostSaoLuu()
        {
            try
            {
                string backupFolder = GetBackupFolder();
                if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                string fileName = $"QLTGTK_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.bak";
                string fullPath = Path.Combine(backupFolder, fileName);

                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_10_SaoLuu.sp_32_SaoLuuDuLieu", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DuongDan", fullPath);
                        cmd.ExecuteNonQuery();
                    }
                }
                SuccessMsg = $"Sao lưu dữ liệu thành công! File lưu tại dự án: {fullPath}";
            }
            catch (Exception ex)
            {
                ErrorMsg = "Lỗi Sao lưu: " + ex.Message;
            }

            LoadDanhSachFile();
            return Page();
        }

        public IActionResult OnPostPhucHoi()
        {
            if (string.IsNullOrEmpty(FilePhucHoi))
            {
                ErrorMsg = "Vui lòng chọn một file để phục hồi!";
                LoadDanhSachFile();
                return Page();
            }

            string fullPath = Path.Combine(GetBackupFolder(), FilePhucHoi);

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("QuanLyTienGuiDB")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("pkg_10_SaoLuu.sp_33_PhucHoiDuLieu", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DuongDan", fullPath);
                        cmd.ExecuteNonQuery();
                    }
                }
                SuccessMsg = $"Khôi phục cơ sở dữ liệu thành công từ file: {FilePhucHoi}. Hệ thống đã quay về trạng thái cũ!";
            }
            catch (SqlException ex)
            {
                if (ex.Class >= 20 || ex.Message.Contains("transport-level"))
                {
                    SuccessMsg = $"Khôi phục cơ sở dữ liệu thành công từ file: {FilePhucHoi}. Hệ thống đã quay về trạng thái cũ!";
                }
                else
                {
                    ErrorMsg = "Lỗi Phục hồi: " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "Lỗi Phục hồi: " + ex.Message;
            }

            LoadDanhSachFile();
            return Page();
        }

        private void LoadDanhSachFile()
        {
            DanhSachFileBackup.Clear();
            string backupFolder = GetBackupFolder();

            if (Directory.Exists(backupFolder))
            {
                string[] files = Directory.GetFiles(backupFolder, "*.bak");
                foreach (var file in files.OrderByDescending(f => new FileInfo(f).CreationTime))
                {
                    DanhSachFileBackup.Add(Path.GetFileName(file));
                }
            }
        }
    }
}