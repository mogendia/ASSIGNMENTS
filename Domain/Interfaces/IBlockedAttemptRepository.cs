using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBlockedAttemptRepository
    {
        Task AddAttemptAsync(BlockedAttempt attempt);
        Task<(List<BlockedAttempt> Attempts, int Total)> GetBlockedAttemptsAsync(
            int page, int pageSize);
    }
}
