using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ApplicationFactory<T> : WebApplicationFactory<T>
    where T : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddDbContext<TeamsLektion5.ApplicationContext>(options =>
            {
                var path = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                );
                options.UseSqlite($"Data source={Path.Join(path, "TestDb.db")}");
            });

            services
                .AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, MyAuthHandler>(
                    "TestScheme",
                    options => { }
                );

            var context = CreateApplicationContext(services);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var user = new TeamsLektion5.User();
            user.Id = "my-test-id";
            user.Email = "my name";
            context.Users.Add(user);
            context.SaveChanges();
        });
    }

    static TeamsLektion5.ApplicationContext CreateApplicationContext(IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();

        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TeamsLektion5.ApplicationContext>();
    }
}

public class MyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "my-test-id"),
            new Claim(ClaimTypes.Name, "my name"),
            new Claim(ClaimTypes.Role, "admin")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}
