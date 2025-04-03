using ESGanalyzer.Backend.Data;
using ESGanalyzer.Backend.DTOs;
using ESGanalyzer.Backend.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class AuthServiceInMemoryTests : IDisposable {
    private readonly ServiceProvider _provider;

    public AuthServiceInMemoryTests() {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString())
            );

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<AuthService>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SECRET_KEY_12345_TO_BE_STORED_IN_SECRETS_LATER" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpireMinutes", "60" }
            })
            .Build();

        services.AddSingleton<IConfiguration>(config);

        _provider = services.BuildServiceProvider();
    }

    public void Dispose() {
        _provider.Dispose();
    }

    [Fact]
    public async Task Register_WithValidCredentials_ReturnsToken() {
        var authService = _provider.GetRequiredService<AuthService>();

        var request = new RegisterRequest {
            Email = "register@example.com",
            Password = "StrongPass123!!"
        };

        var token = await authService.RegisterAsync(request);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken() {
        var userManager = _provider.GetRequiredService<UserManager<IdentityUser>>();
        var email = "test@example.com";
        var password = "StrongPass123!!";
        var user = new IdentityUser { Email = email, UserName = email };
        var createResult = await userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue();

        var authService = _provider.GetRequiredService<AuthService>();

        var token = await authService.LoginAsync(new LoginRequest {
            Email = email,
            Password = password
        });

        token.Should().NotBeNullOrWhiteSpace();
    }

}
