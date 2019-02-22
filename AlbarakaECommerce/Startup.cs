using AlbarakaECommerce.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AlbarakaECommerce.Startup))]
namespace AlbarakaECommerce
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRolesAndUsers();
        }

        // In this method we will create default User roles and Admin user for login   
        public void CreateRolesAndUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<MyRole, int>(new RoleStore<MyRole, int, MyUserRole>(context));
            var userManager = new UserManager<MyUser, int>(new UserStore<MyUser, MyRole, int, MyLogin, MyUserRole, MyClaim>(context));

            // In Startup iam creating first Admin Role and creating a default Admin User 
            if (!roleManager.RoleExists("Admin"))
            {
                // first we create Admin rool
                var role = new MyRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website
                var user = new MyUser();
                user.Email = "furkan@furkan.com";
                user.UserName = "furkan@furkan.com";

                masterEntities entity = new masterEntities();
                var cs = new Customer();
                entity.Customers.Add(cs);
                entity.SaveChanges();
                user.CustomerId = cs.id;

                var chkUser = userManager.Create(user, "ECommerce18");

                //Add default User to Role Admin
                if (chkUser.Succeeded)
                {
                    var result1 = userManager.AddToRole(user.Id, "Admin");
                }
            }

            if (!roleManager.RoleExists("Customer"))
            {
                var role = new MyRole();
                role.Name = "Customer";
                roleManager.Create(role);
            }
        }
    }
}
