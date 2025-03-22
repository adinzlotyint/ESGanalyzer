using ESGanalyzer.Backend.Services;
using ESGanalyzer.Backend.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;

public class AuthServiceTests {
    [Fact]
    public async Task LoginAsync_Should_Return_Token_When_Credentials_Valid() {
        // Arrange
        var mockUser = new IdentityUser {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        var userManager = MockUserManager();
        userManager.Setup(x => x.FindByEmailAsync(mockUser.Email)).ReturnsAsync(mockUser);
        userManager.Setup(x => x.CheckPasswordAsync(mockUser, "validPassword")).ReturnsAsync(true);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SECRET_KEY_12345_TO_BE_STORED_IN_SECRETS_LATER" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpireMinutes", "60" }
            }).Build();

        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(userManager.Object, config, logger);

        var request = new LoginRequest {
            Email = mockUser.Email,
            Password = "validPassword"
        };

        // Act
        var token = await service.LoginAsync(request);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
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