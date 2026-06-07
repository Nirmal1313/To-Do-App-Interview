using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDoApp.Domain.Entities.User;
using ToDoApp.Infrastructure.Data;

namespace ToDoApp.Tests.Controllers.JwtAuthTests
{

    public class JwtWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        public const string SeededEmail = "test@test.com";
        public const string SeededPassword = "Test@123";

        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "TestSecretKeyForJwtThatIsAtLeast32Chars!",
                    ["Jwt:Issuer"] = "ToDoApp",
                    ["Jwt:Audience"] = "ToDoAppUsers",
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                SeedData(db);
            });
        }

        private static void SeedData(AppDbContext db)
        {
            db.Users.RemoveRange(db.Users);
            db.SaveChanges();

            var hasher = new PasswordHasher<User>();
            db.Users.Add(new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Test",
                LastName = "User",
                Email = SeededEmail,
                PasswordHash = hasher.HashPassword(null!, SeededPassword),
            });
            db.SaveChanges();
        }
    }
}
