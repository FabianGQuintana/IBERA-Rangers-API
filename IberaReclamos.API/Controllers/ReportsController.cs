using IberaReclamos.API.Extensions;
using IberaReclamos.Application.DTOs.Report;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Models;
using IberaReclamos.Domain.ConstantsRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IberaReclamos.API.Controllers
{
    /// <summary>Incidentes ambientales reportados por los guardaparques.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.AllStaff)]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Lista reportes con filtros opcionales. Un Admin ve todos; un ranger solo los suyos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReportResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ReportFilterDTO filter)
        {
            if (!User.IsAdmin()) filter.ReportedById = User.GetUserId();
            return Ok(await _reportService.GetFilteredAsync(filter));
        }

        /// <summary>Obtiene un reporte por id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ReportResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var report = await _reportService.GetByIdAsync(id);
            if (!User.IsAdmin() && report.ReportedById != User.GetUserId()) return Forbid();
            return Ok(report);
        }

        /// <summary>
        /// Crea un reporte con conexión (multipart/form-data) con foto opcional en el campo "photo".
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.FieldStaff)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ReportResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] CreateReportDTO dto, IFormFile? photo, CancellationToken cancellationToken)
        {
            var created = await _reportService.CreateAsync(dto, User.GetUserId(), photo.ToFileUpload(), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Sincronización masiva (bulk insert) de los reportes guardados offline. Idempotente por ClientId:
        /// si la app reintenta, los que ya llegaron no se duplican. Después subí cada foto con POST /api/reports/{id}/photo.
        /// </summary>
        [HttpPost("sync")]
        [Authorize(Roles = RoleNames.FieldStaff)]
        [ProducesResponseType(typeof(SyncReportsResultDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Sync([FromBody] SyncReportsDTO dto, CancellationToken cancellationToken)
        {
            return Ok(await _reportService.SyncAsync(dto, User.GetUserId(), cancellationToken));
        }

        /// <summary>Sube o reemplaza la foto de un reporte (multipart/form-data, campo "photo").</summary>
        [HttpPost("{id:guid}/photo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ReportResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile photo, CancellationToken cancellationToken)
        {
            var upload = photo.ToFileUpload();
            if (upload is null) return BadRequest(new ApiErrorResponse { Code = "VALIDATION_FAILED", Message = "Falta la foto.", TraceId = HttpContext.TraceIdentifier });

            return Ok(await _reportService.UploadPhotoAsync(id, upload, User.GetUserId(), User.IsAdmin(), cancellationToken));
        }

        /// <summary>Cambia el estado de un reporte (Pending, Done, Canceled). Solo Admin.</summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(ReportResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateReportStatusDTO dto)
        {
            return Ok(await _reportService.UpdateStatusAsync(id, dto, User.GetUserId().ToString()));
        }
    }
}
