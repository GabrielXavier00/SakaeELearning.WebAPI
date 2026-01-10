using SakaeELearning.WebAPI.Models;

namespace SakaeELearning.WebAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
