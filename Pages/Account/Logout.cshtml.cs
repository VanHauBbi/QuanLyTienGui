using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace QuanLyTienGui.Pages.Account
{
    public class LogoutModel : PageModel
    {
        // Xử lý khi người dùng click vào link (phương thức GET)
        public async Task<IActionResult> OnGetAsync()
        {
            // Xóa sạch Cookie phiên đăng nhập có tên "MyCookieAuth" (đã cấu hình ở Program.cs)
            await HttpContext.SignOutAsync("MyCookieAuth");

            // Chuyển hướng người dùng trở lại màn hình Đăng nhập
            return RedirectToPage("/Account/Login");
        }

        // Xử lý dự phòng nếu gọi bằng Form (phương thức POST)
        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToPage("/Account/Login");
        }
    }
}