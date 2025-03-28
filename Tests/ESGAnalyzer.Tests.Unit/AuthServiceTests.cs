using ESGanalyzer.Backend.DTOs;
using ESGanalyzer.Backend.Exceptions;
using ESGanalyzer.Backend.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;

public class AuthServiceTests {
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly IConfigurationRoot _config;
    private readonly ILogger<AuthService> _logger;

    public AuthServiceTests() {
        _mockUserManager = MockUserManager();
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

        _mockUserManager.Setup(x => x.FindByEmailAsync(mockUser.Email)).ReturnsAsync(mockUser);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(mockUser, "validPassword")).ReturnsAsync(true);

        var service = new AuthService(_mockUserManager.Object, _config, _logger);

        // Act
        var token = await service.LoginAsync(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        _mockUserManager.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
        _mockUserManager.Verify(x => x.CheckPasswordAsync(mockUser, request.Password), Times.Once());
        token.Should().NotBeNullOrWhiteSpace();
        mockUser.Id.Should().Be(jwt.Subject);
        jwt.Claims.Should().ContainSingle(c => c.Type == "email" && c.Value == mockUser.Email);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowError() {
        //Arrange
        var service = new AuthService(_mockUserManager.Object, _config, _logger);

        var request = new LoginRequest {
            Email = "test@test.pl",
            Password = "validPassword"
        };
        
        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((IdentityUser?)null);

        // Act
        var act = () => service.LoginAsync(request);
        //Assert
        await act.Should()
            .ThrowAsync<LoginFailedException>();
        _mockUserManager.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowError() {
        //Arrange
        var mockUser = new IdentityUser {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var service = new AuthService(_mockUserManager.Object, _config, _logger);

        var request = new LoginRequest {
            Email = mockUser.Email,
            Password = "validPassword"
        };


        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(mockUser);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(mockUser, request.Password)).ReturnsAsync(false);

        // Act
        var act = () => service.LoginAsync(request);
        //Assert
        await act.Should()
            .ThrowAsync<LoginFailedException>();
        _mockUserManager.Verify(x => x.CheckPasswordAsync(mockUser, request.Password), Times.Once());
    }

    [Fact]
    public async Task RegisterAsync_WithValidInput_ReturnsJwtToken() {
        //Arrange
        var service = new AuthService(_mockUserManager.Object, _config, _logger);

        var request = new RegisterRequest {
            Email = "test@test.pl",
            Password = "validPassword123@@"
        };

        _mockUserManager.Setup(x => x.CreateAsync(
                    It.Is<IdentityUser>(u =>
                        u.Email == request.Email && u.UserName == request.Email),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);
        //Act
        var token = await service.RegisterAsync(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        //Assert
        token.Should().NotBeNullOrEmpty();
        request.Email.Should().Be(jwt.Claims.First(c => c.Type == "email").Value);
        jwt.Should().NotBeNull();
        (jwt.ValidTo > DateTime.UtcNow).Should().BeTrue();
        _mockUserManager.Verify(x => x.CreateAsync(
                                    It.Is<IdentityUser>(u => u.Email == request.Email),
                                    request.Password), Times.Once());
    }

    [Theory]
    [InlineData("Email already taken")]
    [InlineData("Password too weak")]
    public async Task RegisterAsync_WhenIdentityResultFails_ThrowsError(string errorDesc) {
        //Arrange
        var service = new AuthService(_mockUserManager.Object, _config, _logger);

        var request = new RegisterRequest {
            Email = "test@test.pl",
            Password = "validPassword"
        };

        _mockUserManager.Setup(x => x.CreateAsync(
                    It.Is<IdentityUser>(u =>
                        u.Email == request.Email && u.UserName == request.Email),
                    request.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = errorDesc }));

        //Act
        var act = () => service.RegisterAsync(request);

        //Assert
        var ex = await act.Should().ThrowAsync<RegistrationFailedException>();
        ex.And.Message.Should().Contain(errorDesc);

        _mockUserManager.Verify(x => x.CreateAsync(
            It.Is<IdentityUser>(u => u.Email == request.Email),
            request.Password), Times.Once());

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
            Mock.Of<IServiceProvider>(), // services
            Mock.Of<ILogger<UserManager<IdentityUser>>>()
        );
    }
}