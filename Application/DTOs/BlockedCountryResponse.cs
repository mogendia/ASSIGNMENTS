using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class BlockedCountryResponse
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; }
        public bool IsPermanent { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
