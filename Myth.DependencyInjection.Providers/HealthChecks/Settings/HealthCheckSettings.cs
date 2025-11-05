using Microsoft.Extensions.DependencyInjection;
using Myth.HealthChecks.ValueProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.HealthChecks.Settings {
    public class HealCheckSettings {
        internal IDictionary<string, DatabaseProvider> DatabaseConnections { get; set; } = new Dictionary<string, DatabaseProvider>();

        public HealCheckSettings AddDatabaseCheck(DatabaseProvider provider, string connectionStringKey) {
            DatabaseConnections[connectionStringKey] = provider;
            return this;
        }

        public IHealthChecksBuilder Builder { get; private set; }

        internal bool CheckInternetAccess { get; set; } = true;

        public HealCheckSettings DisableInternetAccessCheck() {
            CheckInternetAccess = false;
            return this;
        }

        public HealCheckSettings(IHealthChecksBuilder builder) {
            Builder = builder;
        }
    }
}
