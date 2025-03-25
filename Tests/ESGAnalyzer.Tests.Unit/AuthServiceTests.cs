using ESGanalyzer.Backend.Services;
using ESGanalyzer.Backend.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

public class AuthServiceTests {
    private readonly Mock<UserManager<IdentityUser>> _mockUser;
    public AuthServiceTests() {
        _mockUser = MockUserManager();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken() {
        // Arrange
        var mockUser = new IdentityUser {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        _mockUser.Setup(x => x.FindByEmailAsync(mockUser.Email)).ReturnsAsync(mockUser);
        _mockUser.Setup(x => x.CheckPasswordAsync(mockUser, "validPassword")).ReturnsAsync(true);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SECRET_KEY_12345_TO_BE_STORED_IN_SECRETS_LATER" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpireMinutes", "60" }
            }).Build();

        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(_mockUser.Object, config, logger);

        var request = new LoginRequest {
            Email = mockUser.Email,
            Password = "validPassword"
        };

        // Act
        var token = await service.LoginAsync(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        _mockUser.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
        _mockUser.Verify(x => x.CheckPasswordAsync(mockUser, request.Password), Times.Once());
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(mockUser.Id, jwt.Subject);
        Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == mockUser.Email);
    }

    private Mock<UserManager<IdentityUser>> MockUserManager() {
        var store = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(
            store.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<IdentityUser>>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            null, // services
            Mock.Of<ILogger<UserManager<IdentityUser>>>()
        );
    }
}