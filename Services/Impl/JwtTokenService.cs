using CateringApp.Models.Entity;
using CateringApp.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CateringApp.Services.Impl
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Pengguna pengguna)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings["Key"] ?? "CateringAppSuperSecretKeyForJwtAuthenticationServiceNet8";
            var issuer = jwtSettings["Issuer"] ?? "CateringApp";
            var audience = jwtSettings["Audience"] ?? "CateringAppUsers";
            var expiryInMinutes = double.TryParse(jwtSettings["ExpiryInMinutes"], out var minutes) ? minutes : 120;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            string role = pengguna.Peran?.NamaPeran ?? pengguna.PeranId switch
            {
                1 => "Pemilik Toko",
                2 => "Karyawan",
                _ => "User"
            };

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, pengguna.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, pengguna.PenggunaId.ToString()),
                new Claim(ClaimTypes.Name, pengguna.Username),
                new Claim(ClaimTypes.Email, pengguna.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
