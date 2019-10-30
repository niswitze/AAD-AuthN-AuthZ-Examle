using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Handler.Auth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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

            //the following two services are a custom token handler and configuration (app settings) handler. Not added by configuration wizard
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
                //added client secret in order to obtain access token through auth code flow. Not added by configuration wizard
                options.ClientSecret = _azureOptions.ClientSecret;
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;

                //changed response type to obtain auth code and id token from signin process. Not added by configuration wizard
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                if (Convert.ToBoolean(_configuration["UseMSAL"]))
                {
                    options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}" + "/v2.0/";
                    options.TokenValidationParameters = new TokenValidationParameters() { NameClaimType = "preferred_username" };

                }
                else
                {
                    options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}";
                    options.Resource = _azureOptions.Resource;
                  
                }
               

                //event handlers for handling when an authCode is received and when the authentication fails. Not added by configuration wizard
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

            //Used to handle when an authCode has been received. Not added by configuration wizard
            private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            {
                var tokenData = GetTokenData(context);

                //used to exchange the authCode for an access and refresh token and then store those tokens in the token cache.
                //Current token cache is in memory and will need to be updated to an external store before production worthy
                //Not added by configuration wizard
                await _tokenHandler.StoreAccessToken(tokenData);

                //used to obtain the idToken passed in on the signin request
                var idToken = context.ProtocolMessage.IdToken;

                //used to tell the handler to skip the code redemption process. Allows for us to handle the exchanging an authCode
                //for an access token
                //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.openidconnect.authorizationcodereceivedcontext.handlecoderedemption?view=aspnetcore-2.2
                context.HandleCodeRedemption(null, idToken);
            }

            //Used to handle any auth failures that may occur during user signin process. Not added by configuration wizard
            private Task OnAuthenticationFailed(AuthenticationFailedContext context)
            {
                context.HandleResponse();
                return Task.FromResult(0);
            }

            /// <summary>
            /// Returns data needed to acquire a new token on behalf of the signed in user that targets the Microsoft Graph API
            /// </summary>
            /// <returns></returns>
            private IDictionary<string, string> GetTokenData(AuthorizationCodeReceivedContext context)
            {
                var dictionary = _configuration.GetSection("AzureAd").GetChildren()
                                                                    .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                                                    .ToDictionary(x => x.Key, x => x.Value);

                dictionary["Code"] = context.TokenEndpointRequest.Code;
                dictionary["userName"] = context.Principal.Identity.Name;
     
                return dictionary;
            }

        }
    }
}
