using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class Rank
    {
        public string Id => $"{Guild}/{User}";

        public DateTimeOffset LastCreditDate { get; set; } = DateTimeOffset.UnixEpoch;

        public ulong Messasges { get; set; }

        public ulong NextXp { get; set; } = 100;

        public ulong Level { get; set; }

        public ulong Xp { get; set; }

        public ulong Guild { get; set; }

        public ulong User { get; set; }

        public string Username { get; set; }

        public string Discriminator { get; set; }

        public string Avatar { get; set; }
    }
}
