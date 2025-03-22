using ESGanalyzer.Backend.Data;
using ESGanalyzer.Backend.DTOs;
using ESGanalyzer.Backend.Helpers;
using ESGanalyzer.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace ESGanalyzer.Backend.Services {
    public class AuthService : IAuthService {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<IdentityUser> userManager, IConfiguration config, ILogger<AuthService> logger) {
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        public async Task<string> RegisterAsync(RegisterRequest request) {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            // To be expanded
            // Should return a reason for denying if caused by compliance reasons
            if (!result.Succeeded) {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("User registration failed: {Errors}", errors);
                throw new Exception("Registration failed. Please check your input.");
            }

            return JwtTokenGenerator.GenerateToken(user.Id, user.Email!, _config);
        }

        public async Task<string> LoginAsync(LoginRequest request) {
            var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new Exception("User not found!");
            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValid) {
                _logger.LogWarning("Login failed for user: {Email}", request.Email);
                throw new Exception("Invalid credentials");
            }


            return JwtTokenGenerator.GenerateToken(user.Id, user.Email!, _config);
        }
    }
}
