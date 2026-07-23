using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApiHandsOn5.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static readonly string SecretKey = "mysuperdupersecret_key_32bytes_long!";

        /// <summary>
        /// Private method to generate a signed JSON Web Token (JWT) with Claims.
        /// </summary>
        private string GenerateJSONWebToken(int userId, string userRole, int expireMinutes = 10)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, userRole),
                new Claim("role", userRole),
                new Claim("UserId", userId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "mySystem",
                audience: "myUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(expireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// GET action method to generate JWT token with default userId 1 and 'Admin' role.
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login()
        {
            string token = GenerateJSONWebToken(1, "Admin", 10);
            return Ok(new { token, userId = 1, role = "Admin", expires = "10 minutes" });
        }

        /// <summary>
        /// GET action method allowing custom role and expiration duration for testing JWT expiration & role-based authorization.
        /// </summary>
        [HttpGet("token")]
        public IActionResult GetCustomToken([FromQuery] int userId = 1, [FromQuery] string role = "Admin", [FromQuery] int expireMinutes = 10)
        {
            string token = GenerateJSONWebToken(userId, role, expireMinutes);
            return Ok(new { token, userId, role, expireMinutes });
        }
    }
}
