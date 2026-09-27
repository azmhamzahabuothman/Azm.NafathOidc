using Azm.NafathOidc.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Azm.NafathOidc
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers <see cref="INafathOidcService"/> with the real Nafath OIDC implementation.
        /// Reads configuration from the "NafathOidc" section by default.
        /// </summary>
        public static IServiceCollection AddNafathOidc(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = NafathOidcOptions.SectionName)
        {
            services.Configure<NafathOidcOptions>(configuration.GetSection(sectionName));
            services.AddScoped<INafathOidcService, NafathOidcService>();
            return services;
        }

        /// <summary>
        /// Registers <see cref="INafathOidcService"/> with the null/mock implementation.
        /// </summary>
        public static IServiceCollection AddNafathOidcNull(this IServiceCollection services)
        {
            services.AddScoped<INafathOidcService, NullNafathOidcService>();
            return services;
        }
    }
}
