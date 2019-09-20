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
                                                         .WithAuthority($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}")
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
                                                          .WithAuthority($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}")
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
                                                          .WithAuthority(new Uri($"{allTokenNeededData["Instance"]}{allTokenNeededData["TenantId"]}"))
                                                          .WithClientSecret(allTokenNeededData["ClientSecret"])
                                                          .Build();

            _msalUserTokenCacheProvider.Initialize(confidentialApp.UserTokenCache, allTokenNeededData["userName"]);

           await confidentialApp.AcquireTokenByAuthorizationCode(_apiScopes, allTokenNeededData["Code"]).ExecuteAsync();

        }
    }


    /// <summary>
    /// MSAL token cache provider interface for user accounts
    /// </summary>
    public interface IMsalUserTokenCacheProvider
    {
        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established. Not needed by all providers.</param>
        void Initialize(ITokenCache tokenCache, string username);

        /// <summary>
        /// Clears the token cache for this user
        /// </summary>
        void Clear();
    }

    //InMemory Cache for storing tokens per user
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
        /// Once the user signes in, this will not be null and can be ontained via a call to Thread.CurrentPrincipal
        /// </summary>
        internal string _signedInUserName;

        /// <summary>Initializes a new instance of the <see cref="MsalPerUserMemoryTokenCacheProvider"/> class.</summary>
        /// <param name="cache">The memory cache instance</param>
        public MsalPerUserMemoryTokenCacheProvider(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established. Not needed by all providers.</param>
        public void Initialize(ITokenCache tokenCache, string username)
        {
            _userTokenCache = tokenCache;
            _userTokenCache.SetBeforeAccess(UserTokenCacheBeforeAccessNotification);
            _userTokenCache.SetAfterAccess(UserTokenCacheAfterAccessNotification);
            _userTokenCache.SetBeforeWrite(UserTokenCacheBeforeWriteNotification);

            _signedInUserName = username;
        }

        /// <summary>
        /// Explores the Claims of a signed-in user (if available) to populate the unique Id of this cache's instance.
        /// </summary>
        /// <returns>The signed in user's object.tenant Id , if available in the ClaimsPrincipal.Current instance</returns>
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
                // Ideally, methods that load and persist should be thread safe.MemoryCache.Get() is thread safe.
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

            byte[] tokenCacheBytes = (byte[])_memoryCache.Get(GetMsalAccountId());
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
        /// To keep the cache, ClaimsPrincipal and Sql in sync, we ensure that the user's object Id we obtained by MSAL after
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
