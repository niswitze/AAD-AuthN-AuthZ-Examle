using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Handler.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;

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
            var dictionary = _configuration.GetSection("AzureAd").GetChildren()
                                                               .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                                               .ToDictionary(x => x.Key, x => x.Value);

            //adding accessToken used to call this api to dictionary of token data
            dictionary["accessToken"] = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            dictionary["resource"] = _configuration["resource"];
            dictionary["userName"] = _httpContextAccessor.HttpContext.User.Identity.Name;
            dictionary["RedirectURI"] = _configuration["RedirectURI"];

            var accessToken = await _tokenHandler.GetAccessTokenOnBehalfOf(dictionary);

            //this initiates a graph service client using ADAL. Using MSAL is documented here https://docs.microsoft.com/en-us/graph/sdks/create-client?context=graph%2Fapi%2F1.0&view=graph-rest-1.0&tabs=CS
            var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));

            //https://docs.microsoft.com/en-us/graph/query-parameters#filter-parameter documentation on using filter parameter with Graph API
            //query options allow passing in OData queries to the api request
            List<QueryOption> options = new List<QueryOption>
            {
                 new QueryOption("$filter", $"userPrincipalName eq '{username}'")
            };

            //user will be of type array which is why FirstOrDefault is used to obtain the actual user model
            var user = await graphServiceClient.Users
                                                    .Request(options)
                                                    .GetAsync();

            return new ObjectResult(user.FirstOrDefault());
        }

    }
}
