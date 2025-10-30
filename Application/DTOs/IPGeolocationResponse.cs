using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class IPGeolocationResponse
    {
        [JsonProperty("ip")]
        public string? Ip { get; set; }

        [JsonProperty("country_code2")]
        public string? CountryCode2 { get; set; }

        [JsonProperty("country_name")]
        public string? CountryName { get; set; }

        [JsonProperty("state_prov")]
        public string? StateProvince { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("isp")]
        public string? Isp { get; set; }
    }
}
