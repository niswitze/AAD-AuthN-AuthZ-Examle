using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Handler.AuthN
{
    public class ADALTokenHandler<T> : ITokenHandler<T>
                                        where T : IDictionary<string,string>
    {
        public async Task<string> GetAccessToken(T allTokenNeededData)
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


            var tokens = await authContext.AcquireTokenByAuthorizationCodeAsync(
                                                                            allTokenNeededData["Code"],
                                                                            new Uri(allTokenNeededData["CallbackPath"]),
                                                                            credential,
                                                                            allTokenNeededData["Resource"]
                                                                         );
        }
    }
}
