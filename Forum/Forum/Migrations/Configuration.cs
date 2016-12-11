namespace Forum.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    public sealed class Configuration : DbMigrationsConfiguration<Forum.Models.ForumDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Forum.Models.ForumDbContext";
        }

        protected override void Seed(Forum.Models.ForumDbContext context)
        {
            if(!context.Roles.Any())
            {
                this.CreateRole("Admin", context);
                this.CreateRole("User", context);
            }

            if(!context.Users.Any())
            {
                this.CreateUser("dimpeev@gmail.com", "Admin", "asdfg123", context);
                this.SetUserRole("dimpeev@gmail.com", "Admin", context);
            }
        }

        private void CreateRole(string roleName, ForumDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var result = roleManager.Create(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }

        private void SetUserRole(string email, string role, ForumDbContext context)
        {
            var userManger = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var userId = context.Users.FirstOrDefault(u => u.Email.Equals(email)).Id;
            var result = userManger.AddToRole(userId, role);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }

        }

        private void CreateUser(string email, string displayName, string pass, ForumDbContext context)
        {
            var userManger = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                DisplayName = displayName
            };

            var result = userManger.Create(user, pass);

            if(!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }
    }
}
