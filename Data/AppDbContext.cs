using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SakaeELearning.WebAPI.Models;

namespace SakaeELearning.WebAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ========================================
        // TABELA PRINCIPAL: Usuários
        // ========================================
        builder.Entity<User>(b =>
        {
            b.ToTable("Users");  // Tabela principal de usuários
            b.Property(u => u.Id).HasColumnName("UserId");
            b.Property(u => u.UserName).HasColumnName("Name");
            b.Property(u => u.NormalizedUserName).HasColumnName("NormalizedName");
            b.Property(u => u.Email).HasColumnName("Email");
            b.Property(u => u.NormalizedEmail).HasColumnName("NormalizedEmail");
            b.Property(u => u.IsActive).HasColumnName("IsActive");
            b.Property(u => u.CreatedAt).HasColumnName("CreatedAt");
        });

        // ========================================
        // TABELAS OPCIONAIS (necessárias pelo Identity, mas ficam vazias no MVP)
        // Se quiser usar Roles no futuro, descomentar e usar normalmente
        // ========================================
        
        // Roles (funções como Admin, User, etc) - ÚTIL para autorização
        builder.Entity<IdentityRole<int>>().ToTable("IdentityRoles");
        
        // Relação User ↔ Role - ÚTIL para autorização
        builder.Entity<IdentityUserRole<int>>().ToTable("IdentityUserRoles");
        
        // Login externo (Google, Facebook) - NÃO USADO NO MVP
        builder.Entity<IdentityUserLogin<int>>().ToTable("IdentityUserLogins");

        // ========================================
        // TABELAS RARAMENTE USADAS (ficam vazias, mas o Identity exige que existam)
        // ========================================
        
        // Claims do usuário - raramente usado
        builder.Entity<IdentityUserClaim<int>>().ToTable("IdentityUserClaims");
        
        // Claims por role - raramente usado
        builder.Entity<IdentityRoleClaim<int>>().ToTable("IdentityRoleClaims");
        
        // Tokens de refresh/confirmação - para fluxos avançados
        builder.Entity<IdentityUserToken<int>>().ToTable("IdentityUserTokens");
    }
}
