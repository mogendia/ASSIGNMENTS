using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBlockedCountryRepository
    {
        Task<bool> AddBlockedCountryAsync(BlockedCountry country);
        Task<bool> RemoveBlockedCountryAsync(string countryCode);
        Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode);
        Task<(List<BlockedCountry> Countries, int Total)> GetAllBlockedCountriesAsync(
            int page, int pageSize, string? searchTerm = null);
        Task<bool> IsCountryBlockedAsync(string countryCode);
    }
}
