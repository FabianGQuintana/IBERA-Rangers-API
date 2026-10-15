using IberaReclamos.API.Extensions;
using IberaReclamos.Application.DTOs.User;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Models;
using IberaReclamos.Domain.ConstantsRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IberaReclamos.API.Controllers
{
    /// <summary>Gestión de usuarios internos (solo Admin).</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Lista todos los usuarios (activos e inactivos).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }

        /// <summary>Obtiene un usuario por id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _userService.GetByIdAsync(id));
        }

        /// <summary>Crea un usuario (ranger, helper o admin). La contraseña se guarda hasheada con BCrypt.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO dto)
        {
            var created = await _userService.CreateAsync(dto, User.GetUserId().ToString());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Actualiza los datos de un usuario (no cambia contraseña ni DNI).</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDTO dto)
        {
            return Ok(await _userService.UpdateAsync(id, dto, User.GetUserId().ToString()));
        }

        /// <summary>Activa o desactiva un usuario (borrado lógico). Un usuario inactivo no puede loguearse.</summary>
        [HttpPatch("{id:guid}/toggle-active")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            return Ok(await _userService.ToggleActiveAsync(id, User.GetUserId()));
        }
    }
}
