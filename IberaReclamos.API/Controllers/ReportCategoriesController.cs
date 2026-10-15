using IberaReclamos.API.Extensions;
using IberaReclamos.Application.DTOs.ReportCategory;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Domain.ConstantsRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IberaReclamos.API.Controllers
{
    /// <summary>Categorías de incidentes (lixiviados, plásticos, incendio...).</summary>
    [ApiController]
    [Route("api/report-categories")]
    [Authorize(Roles = RoleNames.AllStaff)]
    [Produces("application/json")]
    public class ReportCategoriesController : ControllerBase
    {
        private readonly IReportCategoryService _categoryService;

        public ReportCategoriesController(IReportCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>Lista las categorías. La app del ranger las guarda para poder reportar offline.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReportCategoryResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _categoryService.GetAllAsync());
        }

        /// <summary>Crea una categoría nueva (solo Admin).</summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(ReportCategoryResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateReportCategoryDTO dto)
        {
            var created = await _categoryService.CreateAsync(dto, User.GetUserId().ToString());
            return Created($"/api/report-categories/{created.Id}", created);
        }
    }
}
