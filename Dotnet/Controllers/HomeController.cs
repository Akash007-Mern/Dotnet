using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dotnet.Controllers
{
    public class HomeController : Controller

    {
        private readonly CrudContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public HomeController(CrudContext context,
            DataSecurityProvider p, IDataProtectionProvider provider,
            IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector(p.Key);
            _env = env;
        }

        public IActionResult ProfileImage()
        {
            var p = _context.UseLists.Where(u => u.UserId.Equals(Convert.ToInt16(User.Identity!.Name))).FirstOrDefault();
            ViewData["img"] = p.UsePhoto;
            return PartialView("_ProfileImage");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
    
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
