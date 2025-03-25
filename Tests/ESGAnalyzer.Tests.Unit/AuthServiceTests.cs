using Azure.Core;
using ESGanalyzer.Backend.DTOs;
using ESGanalyzer.Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using System.IdentityModel.Tokens.Jwt;

public class AuthServiceTests {
    private readonly Mock<UserManager<IdentityUser>> _mockUser;
    private readonly IConfigurationRoot _config;
    private readonly ILogger<AuthService> _logger;
    private readonly LoginRequest _request;

    public AuthServiceTests() {
        _mockUser = MockUserManager();
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SECRET_KEY_12345_TO_BE_STORED_IN_SECRETS_LATER" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpireMinutes", "60" }
            }).Build();
        _logger = Mock.Of<ILogger<AuthService>>();
        _request = new LoginRequest { };
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken() {
        // Arrange
        var mockUser = new IdentityUser {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        _request.Email = mockUser.Email;
        _request.Password = "validPassword";

        _mockUser.Setup(x => x.FindByEmailAsync(mockUser.Email)).ReturnsAsync(mockUser);
        _mockUser.Setup(x => x.CheckPasswordAsync(mockUser, "validPassword")).ReturnsAsync(true);

        var service = new AuthService(_mockUser.Object, _config, _logger);

        // Act
        var token = await service.LoginAsync(_request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        _mockUser.Verify(x => x.FindByEmailAsync(_request.Email), Times.Once());
        _mockUser.Verify(x => x.CheckPasswordAsync(mockUser, _request.Password), Times.Once());
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(mockUser.Id, jwt.Subject);
        Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == mockUser.Email);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowError() {
        //Arrange
        var service = new AuthService(_mockUser.Object, _config, _logger);
        _request.Email = "missing@example.com";
        _mockUser.Setup(x => x.FindByEmailAsync(_request.Email)).ReturnsAsync((IdentityUser?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(_request));
        Assert.Equal("User not found!", ex.Message);
        _mockUser.Verify(x => x.FindByEmailAsync(_request.Email), Times.Once());
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