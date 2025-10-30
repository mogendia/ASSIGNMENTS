using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly ICountryBlockService _countryBlockService;
        private readonly IBlockedAttemptRepository _attemptRepository;
        private readonly ILogger<IpController> _logger;

        public IpController(
            ICountryBlockService countryBlockService,
            IBlockedAttemptRepository attemptRepository,
            ILogger<IpController> logger)
        {
            _countryBlockService = countryBlockService;
            _attemptRepository = attemptRepository;
            _logger = logger;
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<CountryInfoResponse>> LookupIp([FromQuery] string? ipAddress = null)
        {
            try
            {
                var ip = ipAddress ?? GetCallerIpAddress();
                var result = await _countryBlockService.LookupIpAsync(ip);

                if (result == null)
                    return NotFound(new { message = "Could not find country information for the provided IP" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up IP address");
                return BadRequest(new { message = "Failed to lookup IP address" });
            }
        }

        [HttpGet("check-block")]
        public async Task<ActionResult<IpCheckResponse>> CheckBlock()
        {
            try
            {
                var ipAddress = GetCallerIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var result = await _countryBlockService.CheckIpBlockAsync(ipAddress, userAgent);

                var attempt = new BlockedAttempt
                {
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    CountryCode = result.CountryCode ?? "",
                    IsBlocked = result.IsBlocked,
                    UserAgent = userAgent
                };

                await _attemptRepository.AddAttemptAsync(attempt);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking IP block status");
                return BadRequest(new { message = "Failed to check IP block status" });
            }
        }

        private string GetCallerIpAddress()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
                return forwardedFor.Split(',')[0].Trim();

            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
    }
}
