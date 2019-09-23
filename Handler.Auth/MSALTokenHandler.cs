using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace Handler.Auth
{
    public class MSALTokenHandler<T> : ITokenHandler<T>
                                       where T : IDictionary<string, string>
    {
        private readonly IList<string> _graphScopes = new List<string>()
        {
            "https://graph.microsoft.com/.default"
        };

        private readonly IList<string> _apiScopes = new List<string>()
        {
            "https://M365x640960.onmicrosoft.com/ModernAuth_API/user_impersonation"
        };

        private readonly IMsalUserTokenCacheProvider _msalUserTokenCacheProvider;

        public MSALTokenHandler(IMsalUserTokenCacheProvider msalUserTokenCacheProvider)
        {
            _msalUserTokenCacheProvider = msalUserTokenCacheProvider;
        }

        public async Task<string> GetAccessTokenOnBehalfOf(T allTokenNeededData)
        {
            var confidentialApp = ConfidentialClientApplicationBuilder.Create(allTokenNeededData["ClientId"])
                                                         .WithRedirectUri(allTokenNeededData["RedirectURI"])
                                                         .WithAuthority($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}/v2.0")
                                                         .WithClientSecret(allTokenNeededData["ClientSecret"])
                                                         .Build();

            UserAssertion userAssertion = new UserAssertion(allTokenNeededData["accessToken"]);

            var result = await confidentialApp.AcquireTokenOnBehalfOf(_graphScopes, userAssertion).ExecuteAsync();

            return result.AccessToken;
        }

        public async Task<string> GetAccessTokenSilently(T allTokenNeededData)
        {
            var confidentialApp = ConfidentialClientApplicationBuilder.Create(allTokenNeededData["ClientId"])
                                                          .WithRedirectUri(allTokenNeededData["RedirectURI"])
                                                          .WithAuthority($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}/v2.0")
                                                          .WithClientSecret(allTokenNeededData["ClientSecret"])
                                                          .Build();

            _msalUserTokenCacheProvider.Initialize(confidentialApp.UserTokenCache, allTokenNeededData["userName"]);

            var accounts = await confidentialApp.GetAccountsAsync();

            var result = await confidentialApp.AcquireTokenSilent(_apiScopes, accounts.FirstOrDefault()).ExecuteAsync();

            return result.AccessToken;
        }

        public async Task StoreAccessToken(T allTokenNeededData)
        {
            var confidentialApp = ConfidentialClientApplicationBuilder.Create(allTokenNeededData["ClientId"])
                                                          .WithRedirectUri(allTokenNeededData["RedirectURI"])
                                                          .WithAuthority($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}/v2.0")
                                                          .WithClientSecret(allTokenNeededData["ClientSecret"])
                                                          .Build();

            _msalUserTokenCacheProvider.Initialize(confidentialApp.UserTokenCache, allTokenNeededData["userName"]);

           await confidentialApp.AcquireTokenByAuthorizationCode(_apiScopes, allTokenNeededData["Code"]).ExecuteAsync();

        }
    }

}
