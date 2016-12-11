using Forum.Migrations;
using Microsoft.Owin;
using Owin;
using System.Data.Entity;
using Forum.Models;

[assembly: OwinStartupAttribute(typeof(Forum.Startup))]
namespace Forum
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ForumDbContext, Configuration>());
            ConfigureAuth(app);
        }
    }
}
