using CateringApp.Models.Entity;
using System.Security.Claims;

namespace CateringApp.Services.Interface
{
    public interface IJwtTokenService
    {
        string GenerateToken(Pengguna pengguna);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
