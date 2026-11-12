namespace Entities.Supporting.Seeding
{
    using Domain.Entities.Supporting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Seeds the default roles and the initial admin user.
    /// </summary>
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { StaticUserRoles.ADMIN, StaticUserRoles.CUSTOMER, StaticUserRoles.WASHER };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            const string adminEmail = "admin@tebzacarwash.local";
            const string adminPassword = "ChangeMe@123";

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Tebza"
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, StaticUserRoles.ADMIN);
            }
        }
    }
}