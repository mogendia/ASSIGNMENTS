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
    public class InMemoryTemporalBlockRepository : ITemporalBlockRepository
    {
        private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks = new();

        public Task<bool> AddTemporalBlockAsync(TemporalBlock block)
        {
            return Task.FromResult(_temporalBlocks.TryAdd(block.CountryCode, block));
        }

        public Task<bool> IsTemporallyBlockedAsync(string countryCode)
        {
            if (_temporalBlocks.TryGetValue(countryCode, out var block))
            {
                return Task.FromResult(block.ExpiresAt > DateTime.UtcNow);
            }
            return Task.FromResult(false);
        }

        public Task<TemporalBlock?> GetTemporalBlockAsync(string countryCode)
        {
            _temporalBlocks.TryGetValue(countryCode, out var block);
            return Task.FromResult(block);
        }

        public Task<List<TemporalBlock>> GetExpiredBlocksAsync()
        {
            var expiredBlocks = _temporalBlocks.Values
                .Where(b => b.ExpiresAt <= DateTime.UtcNow)
                .ToList();

            return Task.FromResult(expiredBlocks);
        }

        public Task<List<TemporalBlock>> GetAllTemporalBlocksAsync()
        {
            var allBlocks = _temporalBlocks.Values.ToList();
            return Task.FromResult(allBlocks);
        }

        public Task RemoveTemporalBlockAsync(string countryCode)
        {
            _temporalBlocks.TryRemove(countryCode, out _);
            return Task.CompletedTask;
        }
    }

}
