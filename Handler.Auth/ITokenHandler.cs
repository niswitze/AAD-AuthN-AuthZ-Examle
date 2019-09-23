using System.Threading.Tasks;

namespace Handler.Auth
{
    /// <summary>
    /// Custom service for handling authN requests. How token generation occurs depends on concrete implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITokenHandler<in T>
    {
        /// <summary>
        /// Exchanges an AuthCode for an access and refresh token. Then stores those tokens in the specified cache.
        /// Current token cache is in memory and will need to be updated to an external store before production worthy
        /// </summary>
        /// <param name="allTokenNeededData">All needed parameters to obtain an access token</param>
        Task StoreAccessToken(T allTokenNeededData);

        /// <summary>
        /// Silently looks in the token cache and returns an access token if it exists in said token cache.
        /// Current token cache is in memory and will need to be updated to an external store before production worthy
        /// </summary>
        /// <param name="allTokenNeededData">All needed parameters to obtain an access token</param>
        /// <returns>Access token</returns>
        Task<string> GetAccessTokenSilently(T allTokenNeededData);

        /// <summary>
        /// Obtains an access token on the behalf of the signed in user. Used by ModernAuth_API
        /// </summary>
        /// <param name="allTokenNeededData">All needed parameters to obtain an access token</param>
        /// <returns>Access token</returns>
        Task<string> GetAccessTokenOnBehalfOf(T allTokenNeededData);
    }
}
