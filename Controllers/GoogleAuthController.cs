using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SakaeELearning.WebAPI.DTOs;
using SakaeELearning.WebAPI.Models;
using SakaeELearning.WebAPI.Services;
using System.Security.Claims;

namespace SakaeELearning.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/auth/google")]
    [Tags("Google Auth")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public GoogleAuthController(UserManager<User> userManager, ITokenService tokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        /// <summary>
        /// Iniciar login com Google
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login()
        {
            // O callback deve corresponder exatamente à rota definida abaixo
            var properties = new AuthenticationProperties { RedirectUri = "/api/v1/auth/google/callback" };
            return Challenge(properties, "Google");
        }

        /// <summary>
        /// Callback do Google
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");

            if (!result.Succeeded)
                return BadRequest(new AuthResponseDto { Success = false, Message = "Falha na autenticação externa." });

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest(new AuthResponseDto { Success = false, Message = "Email não recebido do provedor." });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    UserName = (name ?? email.Split('@')[0]).Replace(" ", "") + new Random().Next(1000, 9999).ToString(),
                    Email = email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return BadRequest(new AuthResponseDto { Success = false, Message = $"Erro ao criar usuário: {errors}" });
                }
            }

            var token = _tokenService.GenerateToken(user);
            await HttpContext.SignOutAsync("ExternalCookie");

            // Redirect to Frontend with Token - URL from config
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3002";
            return Redirect($"{frontendUrl}/#/google-callback?token={token}");
        }
    }
}
