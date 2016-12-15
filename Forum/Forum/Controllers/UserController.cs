using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private ForumDbContext db = new ForumDbContext();

        // GET: User
        [HttpGet]
        public ActionResult Index()
        {
            var users = db.Users.ToList();

            ViewBag.Admins = GetAdmins(db);

            return View(users);
        }

        //Get all users with Admin role
        private HashSet<string> GetAdmins(ForumDbContext db)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var users = db.Users.ToList();

            var admins = new HashSet<string>();

            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.Id);
                }
            }

            return admins;
        }

        // GET: /User/Edit
        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var model = new UserViewModel();
            model.Email = user.Email;
            model.DisplayName = user.DisplayName;
            model.Roles = GetUserRoles(user, db);

            return View(model);

        }

        // POST: /User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, UserViewModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));

                if (user == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                int countSelectedRoles = 0;

                foreach (var role in model.Roles)
                {
                    if (role.IsSelected)
                    {
                        countSelectedRoles++;
                    }
                }

                if (countSelectedRoles == 0)
                {
                    ModelState.AddModelError(string.Empty, "User must have a role!");

                    return View(model);
                }
                else if (countSelectedRoles == 2)
                {
                    ModelState.AddModelError(string.Empty, "User must have only 1 role assigned!");
                    return View(model);
                }

                if (user.UserName == User.Identity.GetUserName())
                {
                    bool isSelected = model.Roles.Where(r => r.Name.Equals("Admin")).Select(i => i.IsSelected).FirstOrDefault();

                    if (!isSelected)
                    {
                        ModelState.AddModelError(string.Empty, "You can't remove your own admin rights!");
                        return View(model);
                    }
                }

                user.Email = model.Email;
                user.DisplayName = model.DisplayName;
                user.UserName = model.Email;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    var passwordHasher = new PasswordHasher();
                    var newPasswordHash = passwordHasher.HashPassword(model.Password);
                    user.PasswordHash = newPasswordHash;
                }

                SetUserRoles(user, db, model);

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        //Change selected user role
        private void SetUserRoles(ApplicationUser user, ForumDbContext db, UserViewModel model)
        {
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.IsSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected)
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }

        //Get all available user roles
        private List<Role> GetUserRoles(ApplicationUser user, ForumDbContext db)
        {
            var rolesInDatabase = db.Roles.Select(r => r.Name).OrderBy(r => r).ToList();

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            List<Role> userRoles = new List<Role>();

            foreach(var roleName in rolesInDatabase)
            {
                Role role = new Role() { Name = roleName };
                if(userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }
                userRoles.Add(role);
            }

            return userRoles;
        }

        // GET: /User/Delete
        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Where(u => u.Id.Equals(id)).FirstOrDefault();

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            ViewBag.DisableDeleteButton = false;

            if (user.UserName == User.Identity.GetUserName())
            {
                ModelState.AddModelError(string.Empty, "You can't delete this user. You are currently logged in with it!");
                ViewBag.DisableDeleteButton = true;
            }

            return View(user);
        }

        // POST: /User/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Where(u => u.Id.Equals(id)).FirstOrDefault();

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var userArticles = db.Articles.Where(a => a.AuthorID == user.Id);

            foreach (var article in userArticles)
            {
                db.Articles.Remove(article);
            }

            if (user.ProfileImage != "default.png")
            {
                var fileName = user.ProfileImage;
                var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                System.IO.File.Delete(path);
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Index");
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