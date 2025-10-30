using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ICountryBlockService _countryBlockService;

        public LogsController(ICountryBlockService countryBlockService)
        {
            _countryBlockService = countryBlockService;
        }


        [HttpGet("blocked-attempts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<BlockedAttemptResponse>>> GetBlockedAttempts(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _countryBlockService.GetBlockedAttemptsAsync(page, pageSize);
            return Ok(result);
        }
    }
}
