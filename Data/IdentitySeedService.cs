using Microsoft.AspNetCore.Identity;
using TurnovoFarmer.Models.Constants;

namespace TurnovoFarmer.Data
{
    public static class IdentitySeedService
    {
        public const string DefaultFarmerEmail = "farmer@turnovofarmer.bg";
        public const string DefaultFarmerPassword = "Farmer123!";
        public const string DefaultClientEmail = "client@gmail.com";
        public const string DefaultClientPassword = "Client123!";

        public static async Task SeedRolesAndUsersAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = [AppRoles.Client, AppRoles.Farmer, AppRoles.Admin];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await EnsureUserAsync(userManager, DefaultFarmerEmail, DefaultFarmerPassword, AppRoles.Farmer, AppRoles.Admin);
            await EnsureUserAsync(userManager, DefaultClientEmail, DefaultClientPassword, AppRoles.Client);
        }

        private static async Task EnsureUserAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            params string[] roles)
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
                if (!result.Succeeded)
                {
                    return;
                }
            }

            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
