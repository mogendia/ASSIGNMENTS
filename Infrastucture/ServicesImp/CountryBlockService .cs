using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.ServicesImp
{
    public class CountryBlockService : ICountryBlockService
    {
        private readonly IGeolocationService _geolocationService;
        private readonly IBlockedCountryRepository _blockedCountryRepository;
        private readonly ITemporalBlockRepository _temporalBlockRepository;
        private readonly IBlockedAttemptRepository _attemptRepository;

        public CountryBlockService(
            IGeolocationService geolocationService,
            IBlockedCountryRepository blockedCountryRepository,
            ITemporalBlockRepository temporalBlockRepository,
            IBlockedAttemptRepository attemptRepository)
        {
            _geolocationService = geolocationService;
            _blockedCountryRepository = blockedCountryRepository;
            _temporalBlockRepository = temporalBlockRepository;
            _attemptRepository = attemptRepository;
        }

        public async Task<bool> BlockCountryAsync(string countryCode)
        {
            countryCode = countryCode.ToUpper();
            
            var blockedCountry = new BlockedCountry
            {
                CountryCode = countryCode,
                CountryName = countryCode,
                BlockedAt = DateTime.UtcNow,
                IsPermanent = true
            };

            return await _blockedCountryRepository.AddBlockedCountryAsync(blockedCountry);
        }

        public async Task<bool> UnblockCountryAsync(string countryCode)
        {
            countryCode = countryCode.ToUpper();
            return await _blockedCountryRepository.RemoveBlockedCountryAsync(countryCode);
        }

        public async Task<PagedResponse<BlockedCountryResponse>> GetBlockedCountriesAsync(
            int page, int pageSize, string? searchTerm = null)
        {
            var (countries, total) = await _blockedCountryRepository
                .GetAllBlockedCountriesAsync(page, pageSize, searchTerm);

            var allTemporalBlocks = await _temporalBlockRepository.GetAllTemporalBlocksAsync();
            var activeTemporalBlocks = allTemporalBlocks.Where(t => t.ExpiresAt > DateTime.UtcNow).ToList();

            var allBlocks = new List<BlockedCountryResponse>();

            allBlocks.AddRange(countries.Select(c => new BlockedCountryResponse
            {
                CountryCode = c.CountryCode,
                CountryName = c.CountryName,
                BlockedAt = c.BlockedAt,
                IsPermanent = c.IsPermanent,
                ExpiresAt = null
            }));

            foreach (var tb in activeTemporalBlocks)
            {
                if (!countries.Any(c => c.CountryCode == tb.CountryCode))
                {
                    allBlocks.Add(new BlockedCountryResponse
                    {
                        CountryCode = tb.CountryCode,
                        CountryName = tb.CountryName,
                        BlockedAt = tb.BlockedAt,
                        IsPermanent = false,
                        ExpiresAt = tb.ExpiresAt
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToUpper();
                allBlocks = [.. allBlocks.Where(b =>
                    b.CountryCode.Contains(searchTerm) ||
                    b.CountryName.ToUpper().Contains(searchTerm)
                )];
            }

            var totalFiltered = allBlocks.Count;
            var pagedData = allBlocks
                .OrderBy(b => b.CountryCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PagedResponse<BlockedCountryResponse>
            {
                Data = pagedData,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalFiltered,
                TotalPages = (int)Math.Ceiling(totalFiltered / (double)pageSize)
            };

            return response;
        }

        public async Task<bool> TemporalBlockCountryAsync(string countryCode, int durationMinutes)
        {
            if (durationMinutes < 1 || durationMinutes > 1440)
                throw new ArgumentException("Duration must be between 1 and 1440 minutes");

            countryCode = countryCode.ToUpper();

            if (await _temporalBlockRepository.IsTemporallyBlockedAsync(countryCode))
                throw new InvalidOperationException("Country is already temporarily blocked");

            var temporalBlock = new TemporalBlock
            {
                CountryCode = countryCode,
                CountryName = countryCode, 
                BlockedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes),
                DurationMinutes = durationMinutes
            };

            return await _temporalBlockRepository.AddTemporalBlockAsync(temporalBlock);
        }

        public async Task<CountryInfoResponse?> LookupIpAsync(string ipAddress)
        {
            if (!IsValidIpAddress(ipAddress))
                throw new ArgumentException("Invalid IP address format");

            var countryInfo = await _geolocationService.GetCountryByIpAsync(ipAddress);

            if (countryInfo == null)
                return null;

            return new CountryInfoResponse
            {
                CountryCode = countryInfo.CountryCode,
                CountryName = countryInfo.CountryName,
                IpAddress = countryInfo.IpAddress,
                Isp = countryInfo.Isp,
                City = countryInfo.City,
                Region = countryInfo.Region
            };
        }

        public async Task<IpCheckResponse> CheckIpBlockAsync(string ipAddress, string userAgent)
        {
            if (ipAddress == "::1" || ipAddress == "127.0.0.1")
            {
                var localInfo = new CountryInfoResponse
                {
                    CountryCode = "EG",
                    CountryName = "Localhost (Egypt)",
                    IpAddress = ipAddress,
                    Isp = "Local Network",
                    City = "Cairo",
                    Region = "Local"
                };

                await _attemptRepository.AddAttemptAsync(new BlockedAttempt
                {
                    Id = Guid.NewGuid(),
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    CountryCode = localInfo.CountryCode,
                    IsBlocked = false,
                    UserAgent = userAgent
                });

                return new IpCheckResponse
                {
                    IpAddress = ipAddress,
                    CountryCode = localInfo.CountryCode,
                    CountryName = localInfo.CountryName,
                    IsBlocked = false,
                    Message = "Access allowed (Localhost)"
                };
            }


            var countryInfo = await _geolocationService.GetCountryByIpAsync(ipAddress);

            if (countryInfo == null)
            {
                return new IpCheckResponse
                {
                    IpAddress = ipAddress,
                    IsBlocked = false,
                    Message = "Could not determine country for IP address"
                };
            }

            bool isPermanentlyBlocked = await _blockedCountryRepository
                .IsCountryBlockedAsync(countryInfo.CountryCode);

            bool isTemporallyBlocked = await _temporalBlockRepository
                .IsTemporallyBlockedAsync(countryInfo.CountryCode);

            bool isBlocked = isPermanentlyBlocked || isTemporallyBlocked;

            var attempt = new BlockedAttempt
            {
                Id = Guid.NewGuid(),
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                CountryCode = countryInfo.CountryCode,
                IsBlocked = isBlocked,
                UserAgent = userAgent
            };

            await _attemptRepository.AddAttemptAsync(attempt);

            return new IpCheckResponse
            {
                IpAddress = ipAddress,
                CountryCode = countryInfo.CountryCode,
                CountryName = countryInfo.CountryName,
                IsBlocked = isBlocked,
                Message = isBlocked ? "Access denied - Country is blocked" : "Access allowed"
            };
        }

        public async Task<PagedResponse<BlockedAttemptResponse>> GetBlockedAttemptsAsync(
            int page, int pageSize)
        {
            var (attempts, total) = await _attemptRepository
                .GetBlockedAttemptsAsync(page, pageSize);

            var response = new PagedResponse<BlockedAttemptResponse>
            {
                Data = attempts.Select(a => new BlockedAttemptResponse
                {
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp,
                    CountryCode = a.CountryCode,
                    IsBlocked = a.IsBlocked,
                    UserAgent = a.UserAgent
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return response;
        }

        private bool IsValidIpAddress(string ipAddress)
        {
            var ipv4Pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

                var ipv6Pattern = @"^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4})$";

            return Regex.IsMatch(ipAddress, ipv4Pattern) || Regex.IsMatch(ipAddress, ipv6Pattern);
        }
    }
}