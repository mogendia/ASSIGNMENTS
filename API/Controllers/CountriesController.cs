using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryBlockService _countryBlockService;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(
            ICountryBlockService countryBlockService,
            ILogger<CountriesController> logger)
        {
            _countryBlockService = countryBlockService;
            _logger = logger;
        }


        [HttpPost("block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> BlockCountry([FromBody] BlockCountryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CountryCode))
                return BadRequest(new { message = "Country code is required" });

            try
            {
                var result = await _countryBlockService.BlockCountryAsync(request.CountryCode);

                if (!result)
                    return Conflict(new { message = "Country is already blocked" });

                return Ok(new
                {
                    message = $"Country {request.CountryCode} has been blocked successfully",
                    countryCode = request.CountryCode.ToUpper()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking country {CountryCode}", request.CountryCode);
                return BadRequest(new { message = "Failed to block country" });
            }
        }

        [HttpDelete("block/{countryCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnblockCountry(string countryCode)
        {
            var result = await _countryBlockService.UnblockCountryAsync(countryCode);

            if (!result)
                return NotFound(new { message = $"Country {countryCode} is not blocked" });

            return Ok(new
            {
                message = $"Country {countryCode} has been unblocked successfully",
                countryCode = countryCode.ToUpper()
            });
        }


        [HttpGet("blocked")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<BlockedCountryResponse>>> GetBlockedCountries(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _countryBlockService.GetBlockedCountriesAsync(page, pageSize, search);
            return Ok(result);
        }


        [HttpPost("temporal-block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> TemporalBlockCountry([FromBody] TemporalBlockRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CountryCode))
                return BadRequest(new { message = "Country code is required" });

            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
                return BadRequest(new { message = "Duration must be between 1 and 1440 minutes (24 hours)" });

            try
            {
                var result = await _countryBlockService.TemporalBlockCountryAsync(
                    request.CountryCode,
                    request.DurationMinutes);

                var expiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes);

                return Ok(new
                {
                    message = $"Country {request.CountryCode} has been temporarily blocked",
                    countryCode = request.CountryCode.ToUpper(),
                    durationMinutes = request.DurationMinutes,
                    expiresAt = expiresAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating temporal block for {CountryCode}", request.CountryCode);
                return BadRequest(new { message = "Failed to create temporal block" });
            }
        }
    }
}
