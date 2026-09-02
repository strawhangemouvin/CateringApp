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
                new Claim("UserId", pengguna.PenggunaId.ToString()),
                new Claim(ClaimTypes.Name, pengguna.Username),
                new Claim(ClaimTypes.Email, pengguna.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", pengguna.NamaLengkap)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings["Key"] ?? "CateringAppSuperSecretKeyForJwtAuthenticationServiceNet8";

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateLifetime = false // Ignore expiration date to extract claims
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
