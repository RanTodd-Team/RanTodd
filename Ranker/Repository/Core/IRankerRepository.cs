using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public interface IRankerRepository
    {
        IRanksRepository Ranks { get; }

        IRolesRepository Roles { get; }

        ISettingsRepository Settings { get; }
    }
}
