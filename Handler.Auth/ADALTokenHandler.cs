using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Handler.Auth
{
    public class ADALTokenHandler<T> : ITokenHandler<T>
                                        where T : IDictionary<string,string>
    {
        public async Task<string> GetAccessTokenSilently(T allTokenNeededData)
        {
            var authContext = new AuthenticationContext($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}");
            var result = await authContext.AcquireTokenSilentAsync(allTokenNeededData["Resource"], allTokenNeededData["ClientId"]);
            return result.AccessToken;

        }

        public async Task StoreAccessToken(T allTokenNeededData)
        {
            //currently using in memory for token cache
            ClientCredential credential = new ClientCredential(allTokenNeededData["ClientId"], allTokenNeededData["ClientSecret"]);
            AuthenticationContext authContext = new AuthenticationContext($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}");

            //only used to ensure access and refresh tokens are stored in cache         
            await authContext.AcquireTokenByAuthorizationCodeAsync(
                                                                    allTokenNeededData["Code"],
                                                                    new Uri(allTokenNeededData["redirectURI"]),
                                                                    credential,
                                                                    allTokenNeededData["Resource"]
                                                                    );
        }

        public async Task<string> GetAccessTokenOnBehalfOf(T allTokenNeededData)
        {
            ClientCredential clientCred = new ClientCredential(allTokenNeededData["ClientId"], allTokenNeededData["ClientSecret"]);
         
            UserAssertion userAssertion = new UserAssertion(allTokenNeededData["accessToken"],
                                                            "urn:ietf:params:oauth:grant-type:jwt-bearer",
                                                            allTokenNeededData["userName"]);

            AuthenticationContext authContext = new AuthenticationContext($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}");

            var result = await authContext.AcquireTokenAsync(allTokenNeededData["resource"], clientCred, userAssertion);

            return result.AccessToken;
        }
    }
}
