using IberaReclamos.API.Extensions;
using IberaReclamos.Application.DTOs.Task;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Domain.ConstantsRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IberaReclamos.API.Controllers
{
    /// <summary>Tareas asignadas a los guardaparques.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.AllStaff)]
    [Produces("application/json")]
    public class TasksController : ControllerBase
    {
        private readonly IRangerTaskService _taskService;

        public TasksController(IRangerTaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>Todas las tareas (solo Admin).</summary>
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(IEnumerable<TaskResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _taskService.GetAllAsync());
        }

        /// <summary>Las tareas asignadas al usuario logueado.</summary>
        [HttpGet("mine")]
        [ProducesResponseType(typeof(IEnumerable<TaskResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMine()
        {
            return Ok(await _taskService.GetByAssigneeAsync(User.GetUserId()));
        }

        /// <summary>Asigna una tarea nueva a un ranger (solo Admin).</summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(TaskResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateTaskDTO dto)
        {
            var created = await _taskService.CreateAsync(dto, User.GetUserId());
            return Created($"/api/tasks/{created.Id}", created);
        }

        /// <summary>Cambia el estado de una tarea. El ranger solo puede tocar las suyas.</summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(TaskResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDTO dto)
        {
            return Ok(await _taskService.UpdateStatusAsync(id, dto, User.GetUserId(), User.IsAdmin()));
        }
    }
}
