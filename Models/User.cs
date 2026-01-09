using Microsoft.AspNetCore.Identity;

namespace SakaeELearning.WebAPI.Models
{
    /// <summary>
    /// Usuário do sistema. Herda de IdentityUser para autenticação.
    /// - Id (int): chave primária
    /// - UserName: nome de exibição
    /// - Email: usado para login
    /// </summary>
    public class User : IdentityUser<int>
    {
        // Propriedades herdadas do IdentityUser<int>:
        // - Id (int)
        // - UserName → nome de exibição
        // - Email → login
        // - PasswordHash, SecurityStamp, etc.

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
