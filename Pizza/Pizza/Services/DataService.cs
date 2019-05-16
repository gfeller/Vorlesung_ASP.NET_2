using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Pizza.Services
{
    public class DataService
    {
        private readonly IServiceProvider _provider;


        public DataService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async void EnsureData(string adminPwd)
        {
            using (var serviceScope = _provider.GetService<IServiceScopeFactory>()
                .CreateScope())
            {
                var userManager = serviceScope.ServiceProvider.GetService<UserManager<IdentityUser>>();
                var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

                var role = await roleManager.FindByNameAsync("Administrator");
                if (role == null)
                {
                    role = new IdentityRole() {Name = "Administrator"};
                    await roleManager.CreateAsync(role);
                }

                if (await userManager.FindByNameAsync("admin@admin.ch") == null)
                {
                    var user = new IdentityUser() {UserName = "admin@admin.ch"};

                    await userManager.UpdateSecurityStampAsync(user);
                    await userManager.CreateAsync(user, adminPwd);
                    await userManager.AddToRoleAsync(user, "Administrator");
                }
            }
        }
    }
}