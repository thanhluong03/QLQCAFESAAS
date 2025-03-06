using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QLCAFESAAS.Services;
using QLCAFESAAS.Models;
using Microsoft.EntityFrameworkCore;
using QLCAFESAAS.Models.Repository;

namespace QLCAFESAAS.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserService _userService;
        private readonly DataContext _dataContext;
        private const int MaxFailedAttempts = 3;
        private const int LockoutMinutes = 30;


        public AccountController(UserService userService, DataContext dataContext)
        {
            _userService = userService;
            _dataContext = dataContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("LockoutTime") is string lockoutTimeStr)
            {
                var lockoutTime = DateTime.Parse(lockoutTimeStr);
                if (DateTime.Now < lockoutTime)
                {
                    TempData["ErrorMessage"] = $"Truy cập đã bị khóa, thử lại sau {lockoutTime}";
                    return RedirectToAction("AccessDenied", "Home");
                }
                else
                {
                    HttpContext.Session.Remove("FailedAttempts");
                    HttpContext.Session.Remove("LockoutTime");
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userService.AuthenticateAsync(username, password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Role, user.Permission.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity));

                // Điều hướng theo vai trò
                switch (user.Permission.Role.ToLower())
                {
                    case "admin":
                        return RedirectToAction("Index", "Admin");
                    case "manager":
                        return RedirectToAction("Index", "Manager");
                    case "staff":
                        return RedirectToAction("Index", "Staff");
                    default:
                        return RedirectToAction("AccessDenied", "Home");
                }
            }

            int failedAttempts = HttpContext.Session.GetInt32("FailedAttempts") ?? 0;
            failedAttempts++;
            HttpContext.Session.SetInt32("FailedAttempts", failedAttempts);

            if (failedAttempts >= MaxFailedAttempts)
            {
                var lockoutEndTime = DateTime.Now.AddMinutes(LockoutMinutes);
                HttpContext.Session.SetString("LockoutTime", lockoutEndTime.ToString());
                TempData["ErrorMessage"] = "Đăng nhập thất bại quá nhiều lần, bạn sẽ bị khóa 30 phút";
                return RedirectToAction("AccessDenied", "Home");
            }

            TempData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserModel model)
        {
            try
            {
                var user = await _userService.Register(model.UserName, model.Password, model.Email);
                TempData["InfoMessage"] = "Đăng ký thành công, vui lòng đăng nhập";
                return RedirectToAction("Index", "Account");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmLogout([FromBody] LogoutRequestModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Password))
            {
                return Json(new { success = false, message = "Vui lòng nhập mật khẩu!" });
            }

            string username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "Phiên đăng nhập không hợp lệ!" });
            }

            var user = _dataContext.Users.Include(u => u.Permission).FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại!" });
            }

            if (user.Password != model.Password)
            {
                return Json(new { success = false, message = "Mật khẩu không đúng!" });
            }

            // Xác định trang đích dựa trên quyền của người dùng
            string redirectUrl = Url.Action("Index", "Manager");

            // Xóa session cũ
            HttpContext.Session.Clear();

            // Tạo lại phiên đăng nhập với quyền cũ
            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        new(ClaimTypes.Name, user.UserName),
        new(ClaimTypes.Role, user.Permission.Role)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Json(new { success = true, redirectUrl = redirectUrl });
        }

        public class LogoutRequestModel
        {
            public string Password { get; set; }
        }
    }
}
