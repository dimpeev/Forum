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
        [ValidateAntiForgeryToken]
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
                article.DateCreated = DateTime.Now;
                article.Tags = new HashSet<Tag>();

                if (!string.IsNullOrEmpty(model.Tags))
                {
                    SetArticleTags(article, model);
                }

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
        public ActionResult ViewThreads(int? id, int? page)
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

            int currentPage = 0;

            if (page == null || page < 1)
            {
                currentPage = 1;
            }
            else
            {
                currentPage = page.Value;
            }

            int threadsPerPage = 5;

            ViewBag.Title = db.Categories.Where(c => c.Id == id).FirstOrDefault().CategoryName;

            if (db.Articles.Where(a => a.CategoryId == id).Count() > 5)
            {
                ViewBag.NumberOfPages = Math.Ceiling((double)db.Articles.Where(a => a.CategoryId == id).Count() / (double)threadsPerPage);
            }
            else
            {
                ViewBag.NumberOfPages = 0;
            }

            var articles = db.Articles.Where(a => a.CategoryId == id).Include(a => a.Tags).OrderByDescending(d => d.IsImportant).ThenByDescending(i => i.DateCreated).Skip((currentPage * threadsPerPage) - threadsPerPage).Take(threadsPerPage).ToList();

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

            var articles = db.Articles.Where(a => a.AuthorID == id).Include(a => a.Answers).OrderByDescending(d => d.DateCreated).ToList();

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

            var article = db.Articles.Where(a => a.Id == id).FirstOrDefault();

            if (article == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

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
        [ValidateAntiForgeryToken]
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
                answer.DateCreated = DateTime.Now;

                db.Answers.Add(answer);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = id.GetValueOrDefault() });
            }

            return View(model);
        }

        // GET: Article/DeleteThread
        [HttpGet]
        [Authorize]
        public ActionResult DeleteThread(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var article = db.Articles.Where(a => a.Id == id).Include(a => a.Tags).FirstOrDefault();

            if (article == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("ViewThreads", new { id = article.CategoryId });
            }

            return View(article);
        }

        // POST: Article/DeleteThread
        [HttpPost]
        [Authorize]
        [ActionName("DeleteThread")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteThreadConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var article = db.Articles.Where(a => a.Id == id).FirstOrDefault();

            if (article == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("ViewThreads", new { id = article.CategoryId });
            }

            var answers = db.Answers.Where(a => a.ArticleId.Equals(article.Id)).ToList();

            foreach(var answer in answers)
            {
                db.Answers.Remove(answer);
            }

            article.Tags.Clear();

            db.Articles.Remove(article);

            db.SaveChanges();

            return RedirectToAction("ViewThreads", new { id = article.CategoryId });
        }

        // GET: Article/DeleteAnswer
        [HttpGet]
        [Authorize]
        public ActionResult DeleteAnswer(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var answer = db.Answers.Where(a => a.Id == id).FirstOrDefault();

            if (answer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!IsAuthorisedToEditAnswer(answer))
            {
                return RedirectToAction("Details", new { id = answer.ArticleId });
            }

            return View(answer);
        }

        // POST: Article/DeleteAnswer
        [HttpPost]
        [Authorize]
        [ActionName("DeleteAnswer")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAnswerConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var answer = db.Answers.Where(a => a.Id == id).FirstOrDefault();

            if (answer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!IsAuthorisedToEditAnswer(answer))
            {
                return RedirectToAction("Details", new { id = answer.ArticleId });
            }

            db.Answers.Remove(answer);

            db.SaveChanges();

            return RedirectToAction("Details", new { id = answer.ArticleId });
        }

        // GET: Article/EditThread
        [HttpGet]
        [Authorize]
        public ActionResult EditThread(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var article = db.Articles.Where(a => a.Id == id).FirstOrDefault();

            if (article == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!IsAuthorisedToEditArticle(article))
            {
                return RedirectToAction("ViewThreads", new { id = article.CategoryId });
            }

            var articleToEdit = new ArticleEditViewModel();
            articleToEdit.Id = article.Id;
            articleToEdit.Title = article.Title;
            articleToEdit.Content = article.Content;
            articleToEdit.CategoryId = article.CategoryId;
            articleToEdit.Categories = db.Categories.ToList();
            articleToEdit.Tags = string.Join(", ", article.Tags.Select(t => t.Name));

            return View(articleToEdit);
        }

        // POST: Article/EditThread
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult EditThread(int? id, ArticleEditViewModel model)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var article = db.Articles.Where(a => a.Id == id).FirstOrDefault();

            if (article == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!IsAuthorisedToEditArticle(article))
            {
                return RedirectToAction("Details", new { id = article.Id });
            }

            if (ModelState.IsValid)
            {
                article.Title = model.Title;
                article.Content = model.Content;
                article.CategoryId = model.CategoryId;
                article.LastEdited = DateTime.Now;

                if (!string.IsNullOrEmpty(model.Tags))
                {
                    EditArticleTagsIn(article, model);
                }
                else
                {
                    article.Tags = new HashSet<Tag>();
                }

                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Details", new { id = article.Id });
            }

            model.Categories = db.Categories.ToList();

            return View(model);
        }

        // GET: Article/EditAnswer
        [HttpGet]
        [Authorize]
        public ActionResult EditAnswer(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var answer = db.Answers.Where(a => a.Id == id).FirstOrDefault();

            if (answer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!IsAuthorisedToEditAnswer(answer))
            {
                return RedirectToAction("Details", new { id = answer.ArticleId });
            }

            var answerToEdit = new EditAnswerViewModel();
            answerToEdit.Content = answer.Content;
            answerToEdit.ArticleId = answer.ArticleId;

            return View(answerToEdit);
        }

        // POST: Article/EditAnswer
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult EditAnswer(int? id, EditAnswerViewModel model)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var answer = db.Answers.Where(a => a.Id == id).FirstOrDefault();

            if (answer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (!IsAuthorisedToEditAnswer(answer))
            {
                return RedirectToAction("Details", new { id = answer.ArticleId });
            }

            if (ModelState.IsValid)
            {
                answer.Content = model.Content;
                answer.LastEdited = DateTime.Now;

                db.Entry(answer).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Details", new { id = answer.ArticleId });
            }

            model.ArticleId = answer.ArticleId;

            return View(model);
        }

        // GET: Article/ViewPostsByTag
        [HttpGet]
        public ActionResult ViewPostsByTag(int? tag)
        {
            if (tag == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var articles = db.Articles.Where(a => a.Tags.Any(t => tag == t.Id)).ToList();

            if (articles.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            ViewBag.Tag = db.Tags.Where(t => t.Id == tag).Select(t => t.Name).FirstOrDefault();

            return View(articles);
        }

        private bool IsAuthorisedToEditArticle(Article article)
        {
            bool isAuthor = article.IsUserAuthor(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");

            return isAuthor || isAdmin;
        }

        private bool IsAuthorisedToEditAnswer(Answer answer)
        {
            bool isAuthor = answer.IsUserAuthor(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");

            return isAuthor || isAdmin;
        }

        private void SetArticleTags(Article article, ArticleCreateViewModel model)
        {
            var tagsString = model.Tags.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct();

            article.Tags.Clear();

            foreach (var tagString in tagsString)
            {
                Tag tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    db.Tags.Add(tag);
                }

                article.Tags.Add(tag);
            }
        }

        private void EditArticleTagsIn(Article article, ArticleEditViewModel model)
        {
            var tagsString = model.Tags.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct();

            article.Tags.Clear();

            foreach (var tagString in tagsString)
            {
                Tag tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    db.Tags.Add(tag);
                }

                article.Tags.Add(tag);
            }
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