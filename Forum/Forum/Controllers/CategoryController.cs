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

    [Authorize(Roles ="Admin")]
    public class CategoryController : Controller
    {
        private ForumDbContext db = new ForumDbContext();

        // GET: Category
        [HttpGet]
        public ActionResult Index()
        {
            var categories = db.Categories.ToList();

            return View(categories);
        }

        // GET: /Category/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                var result = db.Categories.FirstOrDefault(c => c.CategoryName.ToLower() == category.CategoryName.ToLower() && c.Id != category.Id);
                if (result == null)
                {
                    category.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                    db.Categories.Add(category);
                    db.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Category with the same name already exists! Please choose another name!");
                    return View(category);
                }

                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: /Category/Edit
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(category);

        }

        // POST: /Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {

                var result = db.Categories.FirstOrDefault(c => c.CategoryName.ToLower() == category.CategoryName.ToLower() && c.Id != category.Id);
                if (result == null)
                {
                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Category with the same name already exists! Please choose another name!");
                    return View(category);
                }
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: /Category/Delete
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var category = db.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(category);

        }

        // POST: /Category/Delete
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var category = db.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var isEmpty = db.Articles.Where(c => c.CategoryId == id).Count();

            if(isEmpty > 0)
            {
                ModelState.AddModelError(string.Empty, "You can not delete a category which contains threads!");

                return View(category);
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}