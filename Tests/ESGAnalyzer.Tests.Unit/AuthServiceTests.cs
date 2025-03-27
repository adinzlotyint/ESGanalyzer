using Azure.Core;
using ESGanalyzer.Backend.DTOs;
using ESGanalyzer.Backend.Exceptions;
using ESGanalyzer.Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;

public class AuthServiceTests {
    private readonly Mock<UserManager<IdentityUser>> _mockUser;
    private readonly IConfigurationRoot _config;
    private readonly ILogger<AuthService> _logger;

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
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken() {
        // Arrange
        var mockUser = new IdentityUser {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        var request = new LoginRequest {
            Email = mockUser.Email,
            Password = "validPassword"
        };

        _mockUser.Setup(x => x.FindByEmailAsync(mockUser.Email)).ReturnsAsync(mockUser);
        _mockUser.Setup(x => x.CheckPasswordAsync(mockUser, "validPassword")).ReturnsAsync(true);

        var service = new AuthService(_mockUser.Object, _config, _logger);

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

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowError() {
        //Arrange
        var service = new AuthService(_mockUser.Object, _config, _logger);

        var request = new LoginRequest {
            Email = "test@test.pl",
            Password = "validPassword"
        };
        
        _mockUser.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((IdentityUser?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LoginFailedException>(() => service.LoginAsync(request));
        Assert.Equal("User not found!", ex.Message);
        _mockUser.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowError() {
        //Arrange
        var mockUser = new IdentityUser {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var service = new AuthService(_mockUser.Object, _config, _logger);

        var request = new LoginRequest {
            Email = mockUser.Email,
            Password = "validPassword"
        };


        _mockUser.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(mockUser);
        _mockUser.Setup(x => x.CheckPasswordAsync(mockUser, request.Password)).ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LoginFailedException>(() => service.LoginAsync(request));
        Assert.Equal("Invalid credentials", ex.Message);
        _mockUser.Verify(x => x.CheckPasswordAsync(mockUser, request.Password), Times.Once());
    }

    [Fact]
    public async Task RegisterAsync_WithValidInput_ReturnsJwtToken() {
        //Arrange
        var service = new AuthService(_mockUser.Object, _config, _logger);

        var request = new RegisterRequest {
            Email = "test@test.pl",
            Password = "validPassword123@@"
        };

        
        _mockUser.Setup(x => x.CreateAsync(
                    It.Is<IdentityUser>(u =>
                        u.Email == request.Email && u.UserName == request.Email),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);
        //Act
        var token = await service.RegisterAsync(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        //Assert
        Assert.False(String.IsNullOrEmpty(token));
        Assert.NotNull(token);
        Assert.Equal(request.Email, jwt.Claims.First(c => c.Type == "email").Value);
        Assert.NotNull(jwt.Subject);
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
        _mockUser.Verify(x => x.CreateAsync(
                                    It.Is<IdentityUser>(u => u.Email == request.Email),
                                    request.Password), Times.Once());
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsError() {

    }
    [Fact]
    public async Task RegisterAsync_WhenPasswordIsTooWeak_ThrowsError() {

    }
    [Fact]
    public async Task RegisterAsync_WhenIdentityResultFails_ThrowsError() {

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