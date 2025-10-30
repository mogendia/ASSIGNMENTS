using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITemporalBlockRepository
    {
        Task<bool> AddTemporalBlockAsync(TemporalBlock block);
        Task<bool> IsTemporallyBlockedAsync(string countryCode);
        Task<TemporalBlock?> GetTemporalBlockAsync(string countryCode);
        Task<List<TemporalBlock>> GetExpiredBlocksAsync();
        Task RemoveTemporalBlockAsync(string countryCode);
        Task<List<TemporalBlock>> GetAllTemporalBlocksAsync();
    }

}
