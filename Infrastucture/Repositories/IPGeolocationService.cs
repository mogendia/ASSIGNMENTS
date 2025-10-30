using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastucture.Repositories
{
    public class IPGeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IMemoryCache _cache;
        private readonly ILogger<IPGeolocationService> _logger;

        public IPGeolocationService(
            HttpClient httpClient,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<IPGeolocationService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["IPGeolocation:ApiKey"] ?? throw new ArgumentNullException("IPGeolocation:ApiKey");
            _cache = cache;
            _logger = logger;
        }

        public async Task<CountryInfo?> GetCountryByIpAsync(string ipAddress)
        {
            var cacheKey = $"geo_{ipAddress}";
            if (_cache.TryGetValue<CountryInfo>(cacheKey, out var cachedInfo))
            {
                _logger.LogInformation("Cache hit for IP: {IpAddress}", ipAddress);
                return cachedInfo;
            }

            try
            {
                var url = $"https://api.ipgeolocation.io/ipgeo?apiKey={_apiKey}&ip={ipAddress}";
                _logger.LogInformation("Calling IPGeolocation API: {Url}", url.Replace(_apiKey, "***"));

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("IPGeolocation Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("IPGeolocation Response: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("IPGeolocation API returned error: {StatusCode} - {Content}",
                        response.StatusCode, content);
                    return null;
                }

                var geoData = JsonConvert.DeserializeObject<IPGeolocationResponse>(content);

                if (geoData == null)
                {
                    _logger.LogWarning("Failed to deserialize IPGeolocation response");
                    return null;
                }

                var countryInfo = new CountryInfo
                {
                    CountryCode = geoData.CountryCode2 ?? string.Empty,
                    CountryName = geoData.CountryName ?? string.Empty,
                    IpAddress = geoData.Ip ?? ipAddress,
                    Isp = geoData.Isp ?? string.Empty,
                    City = geoData.City ?? string.Empty,
                    Region = geoData.StateProvince ?? string.Empty
                };

                _cache.Set(cacheKey, countryInfo, TimeSpan.FromHours(1));
                _logger.LogInformation("Successfully cached geo data for IP: {IpAddress} - Country: {Country}",
                    ipAddress, countryInfo.CountryName);

                return countryInfo;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for IP lookup: {IpAddress}", ipAddress);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during IP lookup: {IpAddress}", ipAddress);
                return null;
            }
        }
    }
}
