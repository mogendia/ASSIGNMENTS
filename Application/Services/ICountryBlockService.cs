using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ICountryBlockService
    {
        Task<bool> BlockCountryAsync(string countryCode);
        Task<bool> UnblockCountryAsync(string countryCode);
        Task<PagedResponse<BlockedCountryResponse>> GetBlockedCountriesAsync(
            int page, int pageSize, string? searchTerm = null);
        Task<bool> TemporalBlockCountryAsync(string countryCode, int durationMinutes);
        Task<CountryInfoResponse?> LookupIpAsync(string ipAddress);
        Task<IpCheckResponse> CheckIpBlockAsync(string ipAddress, string userAgent);
        Task<PagedResponse<BlockedAttemptResponse>> GetBlockedAttemptsAsync(
            int page, int pageSize);
    }

}
