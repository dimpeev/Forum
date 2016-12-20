using Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class HomeController : Controller
    {

        private ForumDbContext db = new ForumDbContext();

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Article");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: HOME/Search
        [HttpGet]
        public ActionResult Search()
        {

            return RedirectToAction("Index");
        }

        // POST: HOME/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(string search, bool title = false, bool content = false, bool tag = false)
        {

            var searchWords = search.ToLower().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            var articles = new List<Article>();

            if (tag)
            {
                var searchResult = db.Articles.Where(a => a.Tags.Any(t => searchWords.Contains(t.Name.ToLower()))).ToList();
                articles.AddRange(searchResult);
            }

            if (title)
            {
                var searchResult = db.Articles.Where(a => searchWords.Any(word => a.Title.ToLower().Contains(word))).ToList();
                articles.AddRange(searchResult);
            }

            if (content)
            {
                var searchResult = db.Articles.Where(a => searchWords.Any(word => a.Content.ToLower().Contains(word))).ToList();
                articles.AddRange(searchResult);

                searchResult = db.Answers.Where(a => searchWords.Any(word => a.Content.ToLower().Contains(word))).Select(s => s.Article).ToList();
                articles.AddRange(searchResult);

            }

            articles = articles.Distinct().ToList();

            ViewBag.SearchString = search;
            ViewBag.TitleCheckBox = Convert.ToBoolean(title);
            ViewBag.ContentCheckBox = Convert.ToBoolean(content);
            ViewBag.TagsCheckBox = Convert.ToBoolean(tag);

            return View(articles);
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