using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RanTodd
{
    public interface IRanToddRepository
    {
        IRanksRepository Ranks { get; }

        IRolesRepository Roles { get; }

        ISettingsRepository Settings { get; }
    }
}
