using Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class ArticleController : Controller
    {
        private ForumDbContext db = new ForumDbContext();

        // GET: Article
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Article/Create
        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            var model = new ArticleCreateViewModel();
            model.Categories = db.Categories.ToList();

            ViewBag.DisableCreateButton = false;

            if(model.Categories.Count() == 0)
            {
                ViewBag.DisableCreateButton = true;
            }

            return View(model);
        }

        // POST: Article/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(ArticleCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name));

                var article = new Article();
                article.AuthorID = user.Id;
                article.Title = model.Title;
                article.Content = model.Title;
                article.CategoryId = model.CategoryId;
                article.IsImportant = model.IsImportant;
                article.DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                db.Articles.Add(article);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            model.Categories = db.Categories.ToList();

            ViewBag.DisableCreateButton = false;

            if (model.Categories.Count() == 0)
            {
                ViewBag.DisableCreateButton = true;
            }

            return View(model);
        }

        // GET: Article/Edit
        [HttpGet]
        [Authorize]
        public ActionResult Edit()
        {
            return View();
        }

        // GET: Article/Delete
        [HttpGet]
        [Authorize]
        public ActionResult Delete()
        {
            return View();
        }

        // GET: Article/Details
        [HttpGet]
        public ActionResult Details()
        {
            return View();
        }

        private bool IsAuthorisedToEdit(Article article)
        {
            bool isAuthor = article.IsUserAuthor(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");

            return isAuthor || isAdmin;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}