using System;
using System.Collections.Generic;
using System.Linq;
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

                SeedPermissions(context);
                SeedAdminUser(
                    serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
                    serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
                    context,
                    serviceScope.ServiceProvider.GetRequiredService<IConfiguration>()
                ).Wait();
            }
        }

        private static void SeedPermissions(ApplicationDbContext context)
        {
            if (!context.Permissions.Any())
            {
                var permissions = new List<Permission>
                {
                    new Permission { Id = 101, Name = "Get All Users" },
                    new Permission { Id = 102, Name = "Get Admin Profile" },
                    new Permission { Id = 103, Name = "Get User By Id" },
                    new Permission { Id = 104, Name = "Register" },
                    new Permission { Id = 105, Name = "Login" },
                    new Permission { Id = 106, Name = "Update Admin" },
                    new Permission { Id = 107, Name = "Update User" },
                    new Permission { Id = 108, Name = "Delete User" },
                    new Permission { Id = 201, Name = "Get All Devices" },
                    new Permission { Id = 202, Name = "Get Device By Id" },
                    new Permission { Id = 203, Name = "Create Device" },
                    new Permission { Id = 204, Name = "Update Device" },
                    new Permission { Id = 205, Name = "Delete Device" },
                    new Permission { Id = 301, Name = "Get All Attempts" },
                    new Permission { Id = 302, Name = "Get Attempt By Id" },
                    new Permission { Id = 303, Name = "Create Attempt" },
                    new Permission { Id = 304, Name = "Update Attempt" },
                    new Permission { Id = 401, Name = "Get All Road Data By Device Id" },
                    new Permission { Id = 501, Name = "Get All Calculated Data By Device Id" },
                    new Permission { Id = 601, Name = "Get All Road Category" },
                    new Permission { Id = 602, Name = "Get Road Category By Id" },
                    new Permission { Id = 603, Name = "Create Road Category" },
                    new Permission { Id = 604, Name = "Update Road Category" },
                    new Permission { Id = 605, Name = "Delete Road Category" }
                    
                };

                context.Permissions.AddRange(permissions);
                context.SaveChanges();
            }
        }

        private static async Task SeedAdminUser(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration config)
        {
            await EnsureRolesExist(roleManager);

            var adminUsername = config["Admin:Username"];
            var adminEmail = config["Admin:Email"];
            var adminPassword = config["Admin:Password"];

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                throw new ArgumentNullException("Admin username, email, or password is missing or empty in configuration.");
            }

            if (userManager.FindByNameAsync(adminUsername).Result == null)
            {
                var adminUser = new AppUser
                {
                    UserName = adminUsername,
                    Email = adminEmail
                };

                var adminResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (adminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Assign all permissions to Admin
                    var allPermissions = await context.Permissions.ToListAsync();
                    foreach (var permission in allPermissions)
                    {
                        context.UserPermissions.Add(new UserPermission
                        {
                            UserId = adminUser.Id,
                            PermissionId = permission.Id
                        });
                    }
                    await context.SaveChangesAsync();
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
