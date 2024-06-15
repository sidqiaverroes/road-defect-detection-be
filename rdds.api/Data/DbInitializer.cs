using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using rdds.api.Models;

namespace rdds.api.Data
{
    public static class Constants
    {
        public const string DefaultAdminUsername = "superadmin";
        public const string DefaultAdminEmail = "admin@admin.admin";
        public const string DefaultAdminPassword = "rddsapi4_Fun";
    }
    public class DbInitializerLogger
    {
        //for logging
    }

    public static class DbInitializer
    {
        public static void InitializeDb(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<AppUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // Ensure database migrations are applied
                    context.Database.Migrate();

                    // Seed admin user if not exists
                    SeedAdminUser(userManager, roleManager).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<DbInitializerLogger>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                    throw;
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminUsername = Constants.DefaultAdminUsername;
            var adminEmail = Constants.DefaultAdminEmail;
            var adminPassword = Constants.DefaultAdminPassword;

            // Check if admin user exists
            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                // Create admin role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Create admin user
                adminUser = new AppUser
                {
                    UserName = adminUsername,
                    Email = adminEmail
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    // Assign 'Admin' role to the admin user
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    throw new Exception($"Failed to create admin user: {result.Errors.First().Description}");
                }
            }
        }
    }
}