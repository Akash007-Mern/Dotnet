using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Dotnet.Controllers
{
    public class AccountController : Controller
    {
        private readonly CrudContext _context;
        private readonly IDataProtector _protector;
        private readonly IConfiguration _config;

        public AccountController(CrudContext context, DataSecurityProvider p, IDataProtectionProvider provider, IConfiguration config)
        {
            _context = context;
            _protector = provider.CreateProtector(p.Key);
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
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
            // Redirect to dashboard after successful change
            return RedirectToAction("Dashboard");
        }

        /// Forgot Password model

        [HttpGet]

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]

        public IActionResult ForgotPassword(UserListEdit edit)
        {
            if (edit.EmailAddress != null)
            {
                Random r = new Random();
                HttpContext.Session.SetString("token", r.Next(9999).ToString());
                var token = HttpContext.Session.GetString("token");
                var user = _context.UseLists.Where(u => u.EmailAddress
                == edit.EmailAddress).FirstOrDefault();
                if (user != null)
                {
                    SmtpClient s = new()
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        Credentials = new
                        NetworkCredential("akashdhimal42@gmail.com", "wzyt njfx eaze nhnk"),
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network
                    };
                    MailMessage m = new()
                    {
                        From = new MailAddress("akashdhimal42@gmail.com"),
                        Subject = "Forgot Password token",
                        Body = $@"<p class='text-red-800' style='background-color-red;'>Forgot Password</p>
                            <p style='background-color:blue;'> EmailToken={_protector.Protect(token)}</p>:{token}",
                        IsBodyHtml = true
                    };

                    m.To.Add(user.EmailAddress);
                    s.Send(m);
                    // return Json("Success");
                    return RedirectToAction("VerifyToken", new { email = user.EmailAddress });
                }
                else
                {
                    ModelState.AddModelError("", "This email is not registered email");
                    return View(edit);
                }

            }
            return Json("Failed");
        }




        [HttpGet]
        public IActionResult VerifyToken(string email)
        {
            return View(new UserListEdit { EmailAddress = email });
        }

        [HttpPost]
        public IActionResult VerifyToken(UserListEdit e)
        {
            var token = HttpContext.Session.GetString("token");

            if (token == e.EmailToken)
            {
                // protect the token before sending in the URL
                var et = _protector.Protect(e.EmailToken!);
                // pass values as route values (query string)
                return RedirectToAction("ResetPassword", new { email = e.EmailAddress, token = et });
            }

            ModelState.AddModelError("", "Invalid token.");
            return View(e);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return RedirectToAction("ForgotPassword");

            try
            {
                // unprotect token from query and compare with session token
                var unprotected = _protector.Unprotect(token);
                var sessionToken = HttpContext.Session.GetString("token");
                if (sessionToken == unprotected)
                {
                    return View(new ChangePassword { EmailAddress = email });
                }
            }
            catch
            {
                // fall through to redirect
            }

            return RedirectToAction("ForgotPassword");
        }

        [HttpPost]
        public IActionResult ResetPassword(ChangePassword model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            var user = _context.UseLists.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            user.UserPassword = _protector.Protect(model.NewPassword);
            _context.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Login");

        }
    }
}
