using Microsoft.AspNetCore.Identity;

namespace SakaeELearning.WebAPI.Models
{
    public class User : IdentityUser
    {
        public string Document { get; set; } = string.Empty;
    }
}
