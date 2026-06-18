using Dotnet.Models;
using System;
using System.IO;
using System.Linq;
using Dotnet.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Controllers

{

    public class RegisterController : Controller
    {
        private readonly CrudContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public RegisterController(CrudContext context,
            DataSecurityProvider p, IDataProtectionProvider provider,
            IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector(p.Key);
            _env = env;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]

        public IActionResult Register(UserListEdit u)
        {
            //return Json(u);
            try
            {
                var users = _context.UseLists.Where
                    (X => X.EmailAddress == u.EmailAddress)
                    .FirstOrDefault();
                if (users == null)
                {
                    short maxid;
                    if (_context.UseLists.Any())
                        maxid = Convert.ToInt16(_context.UseLists.Max(x => x.UserId) + 1);
                    else
                        maxid = 1;
                    u.UserId = maxid;

                    if (u.UserFile != null)
                    {
                        string fileName = "UserImage" + Guid.NewGuid() + Path.GetExtension(u.UserFile.FileName);
                        string filePath = Path.Combine(_env.WebRootPath, "UserImage", fileName);
                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            u.UserFile.CopyTo(stream);
                        }
                        u.UsePhoto = fileName;
                    }

                    UseList userlist = new()
                    {
                        EmailAddress = u.EmailAddress,
                        FullName = u.FullName,
                        CurrentAddress = u.CurrentAddress,
                        UsePhoto = u.UsePhoto,
                        UserId = u.UserId,
                        UserPassword = _protector.Protect(u.UserPassword),
                        UserRole = u.UserRole,
                    };
                    
                    //return Json(userlist);

                    _context.Add(userlist);

                    _context.SaveChanges();

                    //Return RedirectToAction("Login", "Account");
                    return Json("Register Sucessfully");
                }
                else
                {
                    ModelState.AddModelError("", "User already exist with this email.!");
                    return View(u);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Error = ex.Message,
                    Inner = ex.InnerException?.Message
                });
            }
        }
    }
}
