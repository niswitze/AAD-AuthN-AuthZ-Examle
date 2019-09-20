using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Caching;

namespace Handler.Auth.Extensions
{
    public static class HanndlerAuthExtension
    {
        /// <summary>
        /// Extension to register all services needed for using MSAL as token provider
        /// </summary>
        public static IServiceCollection RegisterMSALServices(this IServiceCollection services)
        {
            //services for using Memory Cache Provider with MSAL
            services.AddSingleton<IMsalUserTokenCacheProvider, MsalPerUserMemoryTokenCacheProvider>();

            //custom service for authentication and authorization. Not added by configuration wizard
            services.AddSingleton<ITokenHandler<IDictionary<string, string>>,
                  MSALTokenHandler<IDictionary<string, string>>>();

            return services;
        }

        /// <summary>
        /// Extension to register all services needed for using ADAL as token provider
        /// </summary>
        public static IServiceCollection RegisterADALServices(this IServiceCollection services)
        {
            //custom service for authentication and authorization. Not added by configuration wizard
            services.AddSingleton<ITokenHandler<IDictionary<string, string>>,
                  ADALTokenHandler<IDictionary<string, string>>>();

            return services;
        }
    }
}
