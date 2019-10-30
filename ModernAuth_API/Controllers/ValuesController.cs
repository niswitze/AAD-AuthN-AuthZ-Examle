using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Handler.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace ModernAuth_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenHandler<IDictionary<string, string>> _tokenHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ValuesController(IConfiguration configuration, 
                                ITokenHandler<IDictionary<string, string>> tokenHandler,
                                IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _tokenHandler = tokenHandler;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET api/values/5
        [HttpGet("{username}")]
        public async Task<ActionResult> Get(string username)
        {
            var tokenData = await GetTokenData();

            var graphAccessToken = await _tokenHandler.GetAccessTokenOnBehalfOf(tokenData);

            var user = await GetUserInfo(username, graphAccessToken);

            return new ObjectResult(user);
        }


        /// <summary>
        /// Returns data needed to acquire a new token on behalf of the signed in user that targets the Microsoft Graph API
        /// </summary>
        /// <returns></returns>
        private async Task<IDictionary<string, string>> GetTokenData()
        {
            var tokenData = _configuration.GetSection("AzureAd").GetChildren()
                                                               .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                                               .ToDictionary(x => x.Key, x => x.Value);

            //adding accessToken used to call this api to dictionary of token data
            tokenData["accessToken"] = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            tokenData["resource"] = _configuration["resource"];
            tokenData["userName"] = _httpContextAccessor.HttpContext.User.Identity.Name;
            tokenData["RedirectURI"] = _configuration["RedirectURI"];


            return tokenData;
        }

        /// <summary>
        /// Returns user's basic profile from the Microsoft Graph API
        /// </summary>
        /// <param name="userName">user to query basic profile for</param>
        /// <param name="graphAccessToken">access token for calling the Microsoft Graph APi</param>
        /// <returns></returns>
        private static async Task<User> GetUserInfo(string userName, string graphAccessToken)
        {
            //IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(clientId)
            //                                                                                                    .WithRedirectUri(redirectUri)
            //                                                                                                    .WithClientSecret(clientSecret) // or .WithCertificate(certificate)
            //                                                                                                    .Build();

            //AuthorizationCodeProvider authProvider = new AuthorizationCodeProvider(confidentialClientApplication, scopes);
            //var graphServiceClient = new GraphServiceClient(authProvider);



            //this initiates a graph service client using a DelegateAuthenticationProvider https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/docs/overview.md#delegateauthenticationprovider
            //Using an MSAL based auth provider is documented here https://docs.microsoft.com/en-us/graph/sdks/create-client?context=graph%2Fapi%2F1.0&view=graph-rest-1.0&tabs=CS
            var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", graphAccessToken);

                return Task.FromResult(0);
            }));

            //https://docs.microsoft.com/en-us/graph/query-parameters#filter-parameter documentation on using filter parameter with Graph API
            //query options allow passing in OData queries to the api request
            List<QueryOption> options = new List<QueryOption>
            {
                 new QueryOption("$filter", $"userPrincipalName eq '{ userName }'")
                 //new QueryOption("$top", "5")
            };

            //user will be an array which is why FirstOrDefault is used to obtain the actual user model
            var user = await graphServiceClient.Users
                                                    .Request(options)
                                                    .GetAsync();
          
            //example of obtaining delta with users
            //var deltaUsers = await graphServiceClient.Users.Delta().Request().GetAsync();

            return user.FirstOrDefault();
        }
    }
}
