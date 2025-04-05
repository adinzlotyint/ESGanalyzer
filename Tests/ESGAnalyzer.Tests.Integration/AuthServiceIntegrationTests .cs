using Azure.Core;
using ESGanalyzer.Backend.Data;
using ESGanalyzer.Backend.DTOs;
using ESGanalyzer.Backend.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

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
    public async Task Register_WithValidCredentials_ReturnsValidToken() {
        //Arrange
        var userManager = _provider.GetRequiredService<UserManager<IdentityUser>>();
        var authService = _provider.GetRequiredService<AuthService>();
        var config = _provider.GetRequiredService<IConfiguration>();
        var request = new RegisterRequest {
            Email = "register@example.com",
            Password = "StrongPass123!!"
        };


        //Act
        var token = await authService.RegisterAsync(request);
        var user = await userManager.FindByEmailAsync(request.Email);
        //Assert
        AssertTokenIsValid(token, config, request.Email, user.Id);
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsValidToken() {
        //Arrange
        var userManager = _provider.GetRequiredService<UserManager<IdentityUser>>();
        var email = "test@example.com";
        var password = "StrongPass123!!";
        var config = _provider.GetRequiredService<IConfiguration>();
        var user = new IdentityUser { Email = email, UserName = email };
        
        //Act
        await userManager.CreateAsync(user, password);

        var authService = _provider.GetRequiredService<AuthService>();

        var token = await authService.LoginAsync(new LoginRequest {
            Email = email,
            Password = password
        });
        

        //Assert
        AssertTokenIsValid(token, config, email, user.Id);
        token.Should().NotBeNullOrWhiteSpace();
    }

    private void AssertTokenIsValid(string token, IConfiguration config, string expectedEmail, string expectedUserId) {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var exp = DateTime.UtcNow + TimeSpan.FromMinutes(Convert.ToDouble(config["Jwt:ExpireMinutes"]));

        jwt.Issuer.Should().Be(config["Jwt:Issuer"]);
        jwt.Audiences.First().Should().Be(config["Jwt:Audience"]);
        jwt.ValidTo.Should().BeCloseTo(exp, precision: TimeSpan.FromSeconds(5));
        jwt.Claims.First(c => c.Type == "email").Value.Should().Be(expectedEmail);
        jwt.Claims.First(c => c.Type == "sub").Value.Should().Be(expectedUserId);
    }
}

