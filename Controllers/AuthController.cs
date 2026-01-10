using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using SakaeELearning.WebAPI.DTOs;
using SakaeELearning.WebAPI.Models;
using SakaeELearning.WebAPI.Services;

namespace SakaeELearning.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Tags("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Registrar novo usuário (com Nome, Email e Senha)
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseDto { Success = false, Message = "Dados inválidos." });

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new AuthResponseDto { Success = false, Message = "Email já cadastrado." });

            var user = new User
            {
                UserName = dto.Name,
                Email = dto.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new AuthResponseDto { Success = false, Message = $"Erro ao criar usuário: {errors}" });
            }

            var token = _tokenService.GenerateToken(user);
            return CreatedAtAction(nameof(Register), new AuthResponseDto
            {
                Success = true,
                Message = "Usuário registrado com sucesso!",
                Token = token,
                User = new UserDto { Id = user.Id, Name = user.UserName!, Email = user.Email!, IsActive = user.IsActive }
            });
        }

        /// <summary>
        /// Fazer login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseDto { Success = false, Message = "Dados inválidos." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new AuthResponseDto { Success = false, Message = "Email ou senha inválidos." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new AuthResponseDto { Success = false, Message = "Email ou senha inválidos." });

            var token = _tokenService.GenerateToken(user);
            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Login realizado com sucesso!",
                Token = token,
                User = new UserDto { Id = user.Id, Name = user.UserName!, Email = user.Email!, IsActive = user.IsActive }
            });
        }

        /// <summary>
        /// Fazer logout
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new AuthResponseDto { Success = true, Message = "Logout realizado com sucesso." });
        }

        /// <summary>
        /// Obter dados do usuário logado
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.UserName!,
                Email = user.Email!,
                IsActive = user.IsActive
            });
        }

    }
}
