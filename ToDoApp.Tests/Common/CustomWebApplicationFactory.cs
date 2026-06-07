using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDoApp.Domain.Entities.User;
using ToDoApp.Infrastructure.Data;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public const string SeededEmail = "test@test.com";
    public const string SeededPassword = "Test@123";

    private readonly string _dbName = Guid.NewGuid().ToString();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    private void SeedData(AppDbContext db)
    {
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();

        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = hasher.HashPassword(null!, "Test@123")
        };

        db.Users.Add(user);
        db.SaveChanges();
    }
}