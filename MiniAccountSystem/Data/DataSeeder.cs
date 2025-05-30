using Microsoft.AspNetCore.Identity;

namespace MiniAccountSystem.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "Accountant", "Viewer" };

            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 🔹 Admin User
            await CreateUserIfNotExists(userManager, "safrina@gmail.com", "Safrina@123", "Admin");

            // 🔹 Accountant User
            await CreateUserIfNotExists(userManager, "accountant1@gmail.com", "Account@123", "Accountant");

            // 🔹 Viewer User
            await CreateUserIfNotExists(userManager, "viewer1@gmail.com", "Viewer@123", "Viewer");
        }

        private static async Task CreateUserIfNotExists(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
