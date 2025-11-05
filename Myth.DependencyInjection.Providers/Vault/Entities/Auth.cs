using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Vault.Entities {
    public class Auth {
        public string ClientToken { get; set; } = null!;
        public string Accessor { get; set; } = null!;
        public List<string> Policies { get; set; } = [];
        public AuthMetadata? Metadata { get; set; }
        public int LeaseDuration { get; set; }
        public bool Renewable { get; set; }
    }
}
