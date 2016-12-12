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

        // GET: Category
        public ActionResult Index()
        {
            if(!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            using (ForumDbContext db = new ForumDbContext())
            {
                var categories = db.Categories.ToList();

                return View(categories);
            }
        }

        // GET: /Category/Create
        [HttpGet]
        public ActionResult Create()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                using (ForumDbContext db = new ForumDbContext())
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
                }
                return RedirectToAction("Index", "Category");
            }
            return View(category);
        }

        // GET: /Category/Edit
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (ForumDbContext db = new ForumDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);
                if (category == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                return View(category);
            }
        }

        // POST: /Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                using (ForumDbContext db = new ForumDbContext())
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
                }
                return RedirectToAction("Index", "Category");
            }

            return View(category);
        }

        // GET: /Category/Delete
        public ActionResult Delete(int? id)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (ForumDbContext db = new ForumDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);
                if (category == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                return View(category);
            }
        }

        // POST: /Category/Delete
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (ForumDbContext db = new ForumDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);
                if (category == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                db.Categories.Remove(category);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}