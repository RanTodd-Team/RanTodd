using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class ConfigJson
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete]
        public string Token { get; set; }

        public ulong? GuildId { get; set; }
    }
}
