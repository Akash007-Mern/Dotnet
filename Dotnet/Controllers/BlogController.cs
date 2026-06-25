using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Controllers
{
    public class BlogController : Controller
    {

        private readonly CrudContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public BlogController(CrudContext context,
            DataSecurityProvider p, IDataProtectionProvider provider,
            IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector(p.Key);
            _env = env;
        }


        public ActionResult Index()
        {
            var blogs = _context.BlogPosts
                .Include(b => b.Author)
                .Select(e => new BlogPostEdit
                {
                    PostId = e.PostId,
                    Tittle = e.Tittle,
                    PostDescription = e.PostDescription,
                    Content = e.Content,
                    PublishedDate = e.PublishedDate,
                    AuthorId = e.AuthorId,
                    UploadUserName = e.Author.FullName,
                    UserProfile = e.Author.UsePhoto,
                    EncId = _protector.Protect(e.PostId.ToString())
                }).ToList();
            return View(blogs);
        }

        //post: BlogController/Create

        public ActionResult AddBlog()
        {
            return View();
        }

        //post: BlogController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult AddBlog(BlogPostEdit edit)
        {
            short maxid;
            try
            {
                //any and max are linq
                //if data is present plus 1.
                if (_context.BlogPosts.Any())
                {
                    maxid = Convert.ToInt16(_context.BlogPosts.Max(x => x.PostId + 1));
                }
                else
                {
                    maxid = 1;
                    edit.PostId = maxid;
                    if (edit.BlogFile != null)
                    {
                        string fileName = Guid.NewGuid() + Path.GetExtension(edit.BlogFile.FileName);
                        // webRootPath is for directory
                        string filePath = Path.Combine(_env.WebRootPath, "BlogImage", fileName);
                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            edit.BlogFile.CopyTo(stream);
                        }
                        edit.Content = fileName;
                    }

                    BlogPost p = new()
                    {
                        PostId = edit.PostId,
                        Tittle = edit.Tittle,
                        Content = edit.Content,
                        PostDescription = edit.PostDescription,
                        PublishedDate = edit.PublishedDate,
                        AuthorId = Convert.ToInt16(User.Identity.Name)
                    };

                    _context.Add(p);
                    _context.SaveChanges();
                    return RedirectToAction("Index");

                }
            }
            catch
            {
                return Json("Error");
            }
            return View();

        }




    }
}


