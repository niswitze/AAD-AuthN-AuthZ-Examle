using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Security.Cryptography;

namespace Handler.Auth
{
    public class MSALTokenHandler<T> : ITokenHandler<T>
                                       where T : IDictionary<string, string>
    {
        private readonly IList<string> _scopes = new List<string>()
        {
            "https://graph.microsoft.com/.default"
        };
       
        public async Task<string> GetAccessTokenOnBehalfOf(T allTokenNeededData)
        {
            var confidentialApp = ConfidentialClientApplicationBuilder.Create(allTokenNeededData["ClientId"])
                                                         .WithRedirectUri(allTokenNeededData["RedirectURI"])
                                                         .WithAuthority($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}")
                                                         .WithClientSecret(allTokenNeededData["ClientSecret"])
                                                         .Build();

            UserAssertion userAssertion = new UserAssertion(allTokenNeededData["accessToken"]);

            var result = await confidentialApp.AcquireTokenOnBehalfOf(_scopes, userAssertion).ExecuteAsync();

            return result.AccessToken;
        }

        public async Task<string> GetAccessTokenSilently(T allTokenNeededData)
        {
            var confidentialApp = ConfidentialClientApplicationBuilder.Create(allTokenNeededData["ClientId"])
                                                          .WithRedirectUri(allTokenNeededData["RedirectURI"])
                                                          .WithAuthority($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}")
                                                          .WithClientSecret(allTokenNeededData["ClientSecret"])
                                                          .Build();

            var accounts = await confidentialApp.GetAccountsAsync();
 
            var result = await confidentialApp.AcquireTokenSilent(_scopes, accounts.FirstOrDefault()).ExecuteAsync();

            return result.AccessToken;
        }

        public async Task StoreAccessToken(T allTokenNeededData)
        {
            var confidentialApp = ConfidentialClientApplicationBuilder.Create(allTokenNeededData["ClientId"])
                                                          .WithRedirectUri(allTokenNeededData["RedirectURI"])
                                                          .WithAuthority(new Uri($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}"))
                                                          .WithClientSecret(allTokenNeededData["ClientSecret"])
                                                          .Build();

            

            await confidentialApp.AcquireTokenByAuthorizationCode(_scopes, allTokenNeededData["Code"]).ExecuteAsync();

        }
    }


   
}
