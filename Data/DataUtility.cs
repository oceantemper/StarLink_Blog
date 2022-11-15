using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StarLink_Blog.Models;

namespace StarLink_Blog.Data
{
    public static class DataUtility
    {

        private const string _adminRole = "Administrator";
        private const string _modRole = "Moderator";
        private const string _adminEmail = "Oceanpena@mailinator.com";
        private const string _modEmail = "moderator@mailinator.com";
        public static string GetConnectionString(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);

        }

        private static string BuildConnectionString(string databaseUrl)
        {

            //provides an object representation of a uniform resource identifier (URI) and easy acess to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //PRovides  a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true


            };

            return builder.ToString();
        }




        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //Obtaining the necesary services based on the IServiceProvider parameter 
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();

            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BlogUser>>();

            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await dbContextSvc.Database.MigrateAsync();

            // SEED DEfault ROLES 
            await SeedRolesAsync(roleManagerSvc);

            //SEED DEfault USERS

            await SeedUsersAsync(dbContextSvc, configurationSvc, userManagerSvc);
        }
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(_adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(_adminRole));
            }

            if (!await roleManager.RoleExistsAsync(_modRole))
            {
                await roleManager.CreateAsync(new IdentityRole(_modRole));
            }
        }

        private static async Task SeedUsersAsync(ApplicationDbContext context, IConfiguration configuration, UserManager<BlogUser> userManager)
        {
            try
            {
                if (!context.Users.Any(u => u.Email == _adminEmail))
                {
                    BlogUser adminUser = new()
                    {
                        Email = _adminEmail,
                        UserName = _adminEmail,
                        FirstName = "Ocean",
                        LastName = "Peña",
                        EmailConfirmed = true,

                    };

                    await userManager.CreateAsync(adminUser, configuration["AdminPassword"] ?? Environment.GetEnvironmentVariable("AdminPassword"));
                    await userManager.AddToRoleAsync(adminUser, _adminRole);
                }


                if (!context.Users.Any(u => u.Email == _modEmail))
                {
                    BlogUser modUser = new()
                    {
                        Email = _modEmail,
                        UserName = _modEmail,
                        FirstName = "Ocean",
                        LastName = "Peña",
                        EmailConfirmed = true,

                    };

                    await userManager.CreateAsync(modUser, configuration["ModeratorPassword"] ?? Environment.GetEnvironmentVariable("ModeratorPassword"));
                    await userManager.AddToRoleAsync(modUser, _modRole);
                }
            }
            catch (Exception)
            {

                throw;
            }

           
        }
    }
}
