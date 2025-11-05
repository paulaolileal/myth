using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.HealthChecks.ValueProviders {
    public enum DatabaseProvider {
        SQLServer = 0,
        PostgreSQL = 1,
        SQLite = 2,
        Memory = 3,
        MongoDB = 4,
        Redis = 5
    }
}
