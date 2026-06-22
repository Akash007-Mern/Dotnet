using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;


namespace Dotnet.Controllers
{
    public class AccountController : Controller
    {
        private readonly CrudContext _context;
        private readonly IDataProtector _protector;

        public AccountController(CrudContext context, DataSecurityProvider p, IDataProtectionProvider provider)
        {
            _context = context;
            _protector = provider.CreateProtector(p.Key);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login(UserListEdit uEdit)
        {
            //return Json(uEdit);
            var users = _context.UseLists.ToList();
            if (users != null)
            {
                var u = users.Where(x => x.EmailAddress.ToUpper().Equals(uEdit.EmailAddress.ToUpper()) && _protector.Unprotect(x.UserPassword).Equals(uEdit.UserPassword)).FirstOrDefault();
                if (u != null)
                {
                    List<Claim> claims = new()
                    {
                        new Claim(ClaimTypes.Name, u.UserId.ToString()),
                        new Claim(ClaimTypes.Role, u.UserRole),
                        new Claim("FullName", u.FullName),
                        new Claim("Image", u.UsePhoto),
                        new Claim("Email", u.EmailAddress),
                        new Claim("Address", u.CurrentAddress),
                        
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");    

        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [Authorize]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!short.TryParse(User?.Identity?.Name, out short userId))
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            var user = _context.UseLists.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            try
            {
                var current = _protector.Unprotect(user.UserPassword);
                if (current != model.CurrentPassword)
                {
                    ModelState.AddModelError("", "Current password is incorrect.");
                    return View(model);
                }
            }
            catch
            {
                ModelState.AddModelError("", "Unable to verify current password.");
                return View(model);
            }

            user.UserPassword = _protector.Protect(model.NewPassword);
            _context.Update(user);
            _context.SaveChanges();

            TempData["Success"] = "Password changed successfully.";
            return View();
        }
    }
}

     
