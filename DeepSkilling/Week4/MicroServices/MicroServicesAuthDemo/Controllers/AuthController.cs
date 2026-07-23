using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MicroServicesAuthDemo.Models;

namespace MicroServicesAuthDemo.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (IsValidUser(model))
            {
                var token = GenerateJwtToken(model.Username, 60);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }

        /// <summary>
        /// Endpoint that generates an expired JWT token for Question 4 verification.
        /// </summary>
        [HttpGet("login-expired")]
        public IActionResult LoginExpired()
        {
            var token = GenerateJwtToken("admin_expired", -5);
            return Ok(new { Token = token, Note = "This token expired 5 minutes ago." });
        }

        private bool IsValidUser(LoginModel model)
        {
            // Direct validation as described in scenario
            return (model.Username == "admin" && model.Password == "password") ||
                   (model.Username == "user" && model.Password == "password");
        }

        private string GenerateJwtToken(string username, int durationInMinutes)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, username == "admin" || username == "admin_expired" ? "Admin" : "User")
            };

            string keyString = _config["Jwt:Key"] ?? "ThisIsASecretKeyForJwtToken_32bytes!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "MyAuthServer",
                audience: _config["Jwt:Audience"] ?? "MyApiUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(durationInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
