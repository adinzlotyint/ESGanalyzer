﻿using ESGanalyzer.Backend.DTOs;
using ESGanalyzer.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace ESGanalyzer.Backend.Controllers {
    [ApiController]
    [Route("/[controller]")]
    public class AuthController : ControllerBase {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request) {
            var token = await _authService.RegisterAsync(request);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request) {
            var token = await _authService.LoginAsync(request);
            return Ok(new { token });
        }
    }
}
