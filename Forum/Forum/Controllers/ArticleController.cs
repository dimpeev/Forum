using Forum.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
            var categories = db.Categories.ToList();
            return View(categories);
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
                article.Content = model.Content;
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

        // GET: Article/ViewThreads
        [HttpGet]
        public ActionResult ViewThreads(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var count = db.Categories.Where(c => c.Id == id).Count();

            if (count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            ViewBag.Title = db.Categories.Where(c => c.Id == id).FirstOrDefault().CategoryName;

            var articles = db.Articles.Where(a => a.CategoryId == id).OrderByDescending(i => i.IsImportant).OrderByDescending(d => d.DateCreated).ToList();

            return View(articles);
        }

        // GET: Article/ViewPostsByUser
        [HttpGet]
        public ActionResult ViewPostsByUser(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var count = db.Users.Where(u => u.Id == id).Count();

            if (count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            ViewBag.AuthorName = db.Users.Where(c => c.Id == id).FirstOrDefault().DisplayName;

            var articles = db.Articles.Where(a => a.AuthorID == id && !a.IsImportant).OrderByDescending(d => d.DateCreated).ToList();

            return View(articles);
        }

        // GET: Article/Details
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var count = db.Articles.Where(a => a.Id == id).Count();

            if (count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var article = db.Articles.Where(a => a.Id == id).FirstOrDefault();

            return View(article);
        }

        // GET: Article/AddAnswer
        [HttpGet]
        [Authorize]
        public ActionResult AddAnswer(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var count = db.Articles.Where(a => a.Id == id).Count();

            if (count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var isImportant = db.Articles.Where(a => a.Id == id).FirstOrDefault().IsImportant;

            if(isImportant)
            {
                return RedirectToAction("Index");
            }

            var model = new AddAnswerViewModel();
            model.ArticleId = id.GetValueOrDefault();

            return View(model);
        }

        // POST: Article/AddAnswer
        [HttpPost]
        [Authorize]
        public ActionResult AddAnswer(int? id, AddAnswerViewModel model)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var count = db.Articles.Where(a => a.Id == id).Count();

            if (count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name));

                var answer = new Answer();
                answer.AuthorID = user.Id;
                answer.Content = model.Content;
                answer.ArticleId = id.GetValueOrDefault();
                answer.DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                db.Answers.Add(answer);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = id.GetValueOrDefault() });
            }

            return View(model);
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