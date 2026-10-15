using IberaReclamos.API.Extensions;
using IberaReclamos.Application.DTOs.Tourist;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Models;
using IberaReclamos.Domain.ConstantsRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IberaReclamos.API.Controllers
{
    /// <summary>Valoraciones, sugerencias y quejas de los turistas.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    [Produces("application/json")]
    public class TouristsController : ControllerBase
    {
        private readonly ITouristService _touristService;

        public TouristsController(ITouristService touristService)
        {
            _touristService = touristService;
        }

        /// <summary>
        /// Endpoint PÚBLICO del formulario de turistas (multipart/form-data): portalId, kind, rating, message,
        /// visitDate, email, clientCode, website (honeypot) y la foto opcional en "photo". Responde { "code": "IB-V1234" }.
        /// </summary>
        [HttpPost("/api/feedback")]
        [AllowAnonymous]
        [EnableRateLimiting("PublicFeedbackPolicy")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FeedbackCreatedDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitFeedback([FromForm] CreateTouristFeedbackDTO dto, IFormFile? photo, CancellationToken cancellationToken)
        {
            var result = await _touristService.SubmitFeedbackAsync(dto, photo.ToFileUpload(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>Lista las valoraciones (panel interno). Filtros: ?portal=laguna&amp;kind=complaint</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TouristResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? portal, [FromQuery] string? kind)
        {
            return Ok(await _touristService.GetFilteredAsync(portal, kind));
        }

        /// <summary>Obtiene una valoración por id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TouristResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _touristService.GetByIdAsync(id));
        }
    }
}
