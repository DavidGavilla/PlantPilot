using PlantCare.Api.Models;

namespace PlantCare.Api.Services.Interfaces.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
