using Calabonga.Microservices.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Raime.Facts.Web.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Raime.Facts.Web.Data
{
    public class DataInitializer
    {
        public async static Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();

            await using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var isExists = context!.GetService<IDatabaseCreator>() 
                is RelationalDatabaseCreator databaseCreator && await databaseCreator.ExistsAsync();

            if(isExists)
            {
                return;
            }

            await context.Database.MigrateAsync();

            var roles = AppData.Roles.ToArray();
            var roleStore = new RoleStore<IdentityRole>(context);

            foreach(var role in roles)
            {
                context.Roles.Any(x => x.Name == role);
                await roleStore.CreateAsync(new IdentityRole(role)
                {
                    NormalizedName = role.ToUpper(),
                });
            }

            const string username = "dev@raime.net";

            if(context.Users.Any(x => x.Email == username))
            {
                return;
            }

            var user = new IdentityUser
            {
                Email = username,
                EmailConfirmed = true,
                NormalizedEmail = username.ToUpper(),
                PhoneNumber = "+79000000",
                UserName = username.Split('@')[1].Split('.')[0],
                PhoneNumberConfirmed = true,
                NormalizedUserName = username.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var passwordHasher = new PasswordHasher<IdentityUser>();
            var passwordHash = passwordHasher.HashPassword(user, "123qwe!@#");

            var userStore = new UserStore<IdentityUser>(context);
            var idetityResult = await userStore.CreateAsync(user);
            if(!idetityResult.Succeeded)
            {
                var errorMessage = string.Join(", ", idetityResult.Errors
                    .Select(x => $"{x.Code}: {x.Description}"));
                throw new MicroserviceDatabaseException(errorMessage);
            }

            var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
            foreach(var role in roles)
            {
                var identityResult = await userManager!.AddToRoleAsync(user, role);
                if(!identityResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", idetityResult.Errors
                        .Select(x => $"{x.Code}: {x.Description}"));
                    throw new MicroserviceDatabaseException(errorMessage);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
