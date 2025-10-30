using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BlockedAttempt
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string UserAgent { get; set; } = string.Empty;
    }
}
