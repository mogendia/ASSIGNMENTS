using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class IpCheckResponse
    {
        public string IpAddress { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
