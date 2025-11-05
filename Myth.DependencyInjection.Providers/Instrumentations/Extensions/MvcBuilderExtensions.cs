using Microsoft.Extensions.DependencyInjection;
using Myth.Instrumentations.Controllers;

namespace Myth.Instrumentations.Extensions {
    public static class MvcBuilderExtensions {
        /// <summary>
        /// Add the default controllers to the MVC builder pipeline
        /// </summary>
        /// <param name="mvcBuilder">The MVC builder pipeline</param>
        /// <returns>The MVC builder pipeline</returns>
        public static IMvcBuilder AddMetricsController(this IMvcBuilder mvcBuilder) {
            mvcBuilder.AddApplicationPart(typeof(MetricsController).Assembly);

            return mvcBuilder;
        }
    }
}
