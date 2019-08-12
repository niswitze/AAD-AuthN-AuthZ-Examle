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
using Newtonsoft.Json;

namespace ModernAuth_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenHandler<IDictionary<string, string>> _tokenHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ValuesController(IHttpClientFactory httpClientFactory, 
                                IConfiguration configuration, 
                                ITokenHandler<IDictionary<string, string>> tokenHandler,
                                IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenHandler = tokenHandler;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET api/values/5
        [HttpGet("{username}")]
        public async Task<ActionResult> Get(string username)
        {
            HttpResponseMessage response = null;
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                //obtain app settings from AzureAd section
                var dictionary = _configuration.GetSection("AzureAd").GetChildren()
                                                                   .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                                                   .ToDictionary(x => x.Key, x => x.Value);
                var urlEndPoint = "/v1.0/users/" + username;

                //adding accessToken used to call this api to dictionary of token data
                dictionary["accessToken"] = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

                dictionary["resource"] = _configuration["resource"];
                dictionary["userName"] = username;

                var accessToken = await _tokenHandler.GetAccessTokenOnBehalfOf(dictionary);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                response = await httpClient.GetAsync(_configuration["resource"] + urlEndPoint);
            }

            var rawBody = await response.Content.ReadAsStringAsync();

            return new ObjectResult(rawBody);
        }

    }
}
