﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Handler.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdAuthenticationBuilderExtensions
    {        
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(_ => { });

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect();
            return builder;
        }

        private class ConfigureAzureOptions: IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;
            private readonly ITokenHandler<IDictionary<string, string>> _tokenHandler;
            private readonly IConfiguration _configuration;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions, 
                                         ITokenHandler<IDictionary<string, string>> tokenHandler,
                                         IConfiguration configuration)
            {
                _azureOptions = azureOptions.Value;
                _tokenHandler = tokenHandler;
                _configuration = configuration;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                options.ClientId = _azureOptions.ClientId;
                options.ClientSecret = _azureOptions.ClientSecret;
                options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}";
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;

                options.Resource = _azureOptions.Resource;

                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                options.Events = new OpenIdConnectEvents
                {
                    OnAuthenticationFailed = OnAuthenticationFailed,
                    OnAuthorizationCodeReceived = OnAuthorizationCodeReceived

                };
            }

            public void Configure(OpenIdConnectOptions options)
            {              
                Configure(Options.DefaultName, options);
            }

            private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            {
               
                var dictionary = _configuration.GetSection("AzureAd").GetChildren()
                                                                    .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                                                    .ToDictionary(x => x.Key, x => x.Value);

                dictionary["Code"] = context.TokenEndpointRequest.Code;
                dictionary["redirectURI"] = _configuration["redirectURI"];

                await _tokenHandler.StoreAccessToken(dictionary);

                var accessToken = await _tokenHandler.GetAccessTokenSilently(dictionary);
                var idToken = context.ProtocolMessage.IdToken;

                context.HandleCodeRedemption(accessToken, idToken);
            }

            private Task OnAuthenticationFailed(AuthenticationFailedContext context)
            {
                context.HandleResponse();
                return Task.FromResult(0);
            }

        }
    }
}