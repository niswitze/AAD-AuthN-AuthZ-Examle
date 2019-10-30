﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder)
            => builder.AddAzureAdBearer(_ => { });

        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureAzureOptions>();
            builder.AddJwtBearer();
            return builder;
        }

        private class ConfigureAzureOptions: IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly AzureAdOptions _azureOptions;
            private readonly IConfiguration _configuration;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions, IConfiguration configuration)
            {
                _azureOptions = azureOptions.Value;
                _configuration = configuration;
            }

            public void Configure(string name, JwtBearerOptions options)
            {

                if (Convert.ToBoolean(_configuration["UseMSAL"]))
                {
                    //validation for bearer token, added by configuration wizard
                    options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}" + "/v2.0/";
                }
                else
                {
                    //validation for bearer token, added by configuration wizard
                    options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}";
                }

                //not added by configuration wizard. Added by author for extra validation
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidAudiences = new List<string>()
                    {
                        _azureOptions.AppIDURL,
                        _azureOptions.ClientId
                    }
                };
            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}
