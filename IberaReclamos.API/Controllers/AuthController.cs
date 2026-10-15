using IberaReclamos.API.Extensions;
using IberaReclamos.Application.DTOs.Auth;
using IberaReclamos.Application.DTOs.User;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IberaReclamos.API.Controllers
{
    /// <summary>Login del personal interno (Admin, Ranger, Helper).</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        /// <summary>Inicia sesión y devuelve el JWT para usar en el header Authorization: Bearer {token}.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthLoginPolicy")]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        /// <summary>Datos del usuario logueado (sirve para validar el token guardado en la app).</summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me()
        {
            var me = await _authService.GetMeAsync(User.GetUserId());
            return Ok(me);
        }

        /// <summary>Cambia la contraseña del usuario logueado.</summary>
        [HttpPut("me/password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            await _userService.ChangePasswordAsync(User.GetUserId(), dto);
            return NoContent();
        }
    }
}
