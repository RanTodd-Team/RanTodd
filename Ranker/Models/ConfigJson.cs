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
        public string Domain { get; set; }

        public ulong? GuildId { get; set; }

        public string[] Prefixes { get; set; }

        public string ClientId { get; set; }
    }
}
