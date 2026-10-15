using IberaReclamos.Application.DTOs.Role;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Domain.ConstantsRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IberaReclamos.API.Controllers
{
    /// <summary>Roles disponibles (para el combo del alta de usuarios).</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoleResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _roleService.GetAllAsync());
        }
    }
}
