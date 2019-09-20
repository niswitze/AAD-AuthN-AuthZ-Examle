using Microsoft.Identity.Client;

namespace Handler.Auth
{
    /// <summary>
    /// MSAL token cache provider interface for user accounts. This interface was modified from 
    /// https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/blob/master/src/Microsoft.Identity.Client.Extensions.Web/TokenCacheProviders/IMsalUserTokenCacheProvider.cs
    /// </summary>
    public interface IMsalUserTokenCacheProvider
    {
        /// <summary>
        /// Clears the token cache for this user
        /// </summary>
        void Clear();

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="username">The signed-in username for whom the cache needs to be established.</param>
        void Initialize(ITokenCache tokenCache, string username);
    }
}