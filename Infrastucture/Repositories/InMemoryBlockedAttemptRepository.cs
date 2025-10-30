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
    public class InMemoryBlockedAttemptRepository : IBlockedAttemptRepository
    {
        private readonly ConcurrentBag<BlockedAttempt> _attempts = [];

        public Task AddAttemptAsync(BlockedAttempt attempt)
        {
            var lastAttempt = _attempts
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefault(a => a.IpAddress == attempt.IpAddress);

            if (lastAttempt != null && (DateTime.UtcNow - lastAttempt.Timestamp).TotalSeconds < 60)
                return Task.CompletedTask;

            _attempts.Add(attempt);
            return Task.CompletedTask;
        }

        public Task<(List<BlockedAttempt> Attempts, int Total)> GetBlockedAttemptsAsync(
            int page, int pageSize)
        {
            var total = _attempts.Count;
            var attempts = _attempts
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult((attempts, total));
        }
    }
}
