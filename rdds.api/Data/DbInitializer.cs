using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using rdds.api.Models;

namespace rdds.api.Data
{
    public class DbInitializer
    {

        public static void InitializeDb(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Database.Migrate();

                SeedAccessTypes(context);
                SeedAdminUser(
                    serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
                    serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
                    context,
                    serviceScope.ServiceProvider.GetRequiredService<IConfiguration>()
                ).Wait();
            }
        }

        private static void SeedAccessTypes(ApplicationDbContext context)
        {
            if (!context.AccessTypes.Any())
            {
                var accessTypes = new List<AccessType>
                {
                    new AccessType { Name = "Admin", Accesses = new List<string> { "read", "write", "update", "delete" } },
                    new AccessType { Name = "User", Accesses = new List<string> { "read" } }
                };

                context.AccessTypes.AddRange(accessTypes);
                context.SaveChanges();
            }
        }

        private static async Task SeedAdminUser(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration config)
{
    // Ensure roles exist
    await EnsureRolesExist(roleManager);

    // Ensure admin user exists
    var userName = config["Admin:Username"];
    var email = config["Admin:Email"];
    var password = config["Admin:Password"];

    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        throw new ArgumentNullException("Admin username, email, or password is missing or empty in configuration.");
    }

    if (userManager.FindByNameAsync(userName).Result == null)
    {
        var adminAccessType = context.AccessTypes.FirstOrDefault(at => at.Name == "Admin");
        if (adminAccessType == null)
        {
            throw new Exception("Admin access type not found. Ensure the AccessTypes are seeded properly.");
        }

        var user = new AppUser
        {
            UserName = userName,
            Email = email,
            AccessTypeId = adminAccessType.Id
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}


        private static async Task EnsureRolesExist(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
