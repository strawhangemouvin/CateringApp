using CateringApp.Models.Entity;

namespace CateringApp.Services.Interface
{
    public interface IJwtTokenService
    {
        string GenerateToken(Pengguna pengguna);
    }
}
