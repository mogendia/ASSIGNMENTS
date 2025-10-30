using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastucture.Repositories
{
    public class InMemoryBlockedCountryRepository : IBlockedCountryRepository
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new();

        public Task<bool> AddBlockedCountryAsync(BlockedCountry country)
        {
            return Task.FromResult(_blockedCountries.TryAdd(country.CountryCode, country));
        }

        public Task<bool> RemoveBlockedCountryAsync(string countryCode)
        {
            return Task.FromResult(_blockedCountries.TryRemove(countryCode, out _));
        }

        public Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode)
        {
            _blockedCountries.TryGetValue(countryCode, out var country);
            return Task.FromResult(country);
        }

        public Task<(List<BlockedCountry> Countries, int Total)> GetAllBlockedCountriesAsync(
            int page, int pageSize, string? searchTerm = null)
        {
            var query = _blockedCountries.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToUpper();
                query = query.Where(c =>
                    c.CountryCode.Contains(searchTerm) ||
                    c.CountryName.ToUpper().Contains(searchTerm));
            }

            var total = query.Count();
            var countries = query
                .OrderBy(c => c.CountryCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult((countries, total));
        }

        public Task<bool> IsCountryBlockedAsync(string countryCode)
        {
            return Task.FromResult(_blockedCountries.ContainsKey(countryCode));
        }
    }
}
