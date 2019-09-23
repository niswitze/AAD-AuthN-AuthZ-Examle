using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;

namespace Handler.Auth
{
    /// <summary>
    /// An implementation of token cache for both Confidential and Public clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext.Session to cache data.
    /// This provider was modified from 
    /// https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/blob/master/src/Microsoft.Identity.Client.Extensions.Web/TokenCacheProviders/InMemory/MsalPerUserMemoryTokenCacheProvider.cs
    /// </summary>
    /// <remarks>https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization</remarks>
    public class MsalPerUserMemoryTokenCacheProvider: IMsalUserTokenCacheProvider
    {
        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        internal IMemoryCache _memoryCache;

        /// <summary>
        /// The internal handle to the client's instance of the Cache
        /// </summary>
        private ITokenCache _userTokenCache;

        /// <summary>
        /// Once the user signes in, this will not be null and will contain the user's username, ex.) admin@contoso.onmicrosoft.com
        /// </summary>
        internal string _signedInUserName;

        /// <summary>Initializes a new instance of the <see cref="MsalPerUserMemoryTokenCacheProvider"/> class.</summary>
        /// <param name="cache">The memory cache instance</param>
        public MsalPerUserMemoryTokenCacheProvider(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        public void Initialize(ITokenCache tokenCache, string username)
        {
            _userTokenCache = tokenCache;
            _userTokenCache.SetBeforeAccess(UserTokenCacheBeforeAccessNotification);
            _userTokenCache.SetAfterAccess(UserTokenCacheAfterAccessNotification);
            _userTokenCache.SetBeforeWrite(UserTokenCacheBeforeWriteNotification);

            _signedInUserName = username;
        }

        /// <summary>
        /// Returns the signed in user's username, if available.
        /// </summary>
        /// <returns>The signed in user's username, if available</returns>
        internal string GetMsalAccountId()
        {
            if (_signedInUserName != null)
            {
                return _signedInUserName;
            }
            return null;
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear()
        {
            _memoryCache.Remove(GetMsalAccountId());
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            SetSignedInUserFromNotificationArgs(args);

            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                // Ideally, methods that load and persist should be thread safe. MemoryCache.Get() is thread safe.
                _memoryCache.Set(GetMsalAccountId(), args.TokenCache.SerializeMsalV3());
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it
        /// changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            string cacheKey = GetMsalAccountId();

            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }

            byte[] tokenCacheBytes = (byte[])_memoryCache.Get(cacheKey);
            args.TokenCache.DeserializeMsalV3(tokenCacheBytes);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
        }

        /// <summary>
        /// To keep the cache we ensure that the user's username we obtained by MSAL after
        /// successful sign-in is set as the key for the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void SetSignedInUserFromNotificationArgs(TokenCacheNotificationArgs args)
        {
            if (_signedInUserName == null && args.Account != null)
            {
                _signedInUserName = args.Account.Username;
            }
        }
    }

}
