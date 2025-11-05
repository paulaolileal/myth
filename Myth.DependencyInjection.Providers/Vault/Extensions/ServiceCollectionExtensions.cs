using Microsoft.Extensions.DependencyInjection;
using Myth.Vault.Interfaces;
using Myth.Vault.Settings;
using Myth.Vault.ValueProviders;

namespace Myth.Vault.Extensions {
    public static class ServiceCollectionExtensions {

        public static IServiceCollection AddHashicorpVault(this IServiceCollection services, Action<VaultSettings> settings) {
            services.Configure<VaultSettings>(settings!);
            services.AddSingleton<ISecretConfiguration, HashiCorpConfigurations>();

            return services;
        }

        public static IServiceCollection AddLocalVault(this IServiceCollection services) {
            services.AddSingleton<ISecretConfiguration, HashiCorpConfigurations>();

            return services;
        }
    }
}
