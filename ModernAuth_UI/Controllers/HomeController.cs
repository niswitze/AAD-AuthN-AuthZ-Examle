using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Handler.AuthN;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ModernAuth_UI.Models;
using MSGraphHandler_ADAL.Models;
using Newtonsoft.Json;

namespace ModernAuth_UI.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenHandler<IDictionary<string, string>> _tokenHandler;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenHandler<IDictionary<string, string>> tokenHandler)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenHandler = tokenHandler;
        }


        public async Task<IActionResult> Index()
        {
           
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                string queryString = "?userName=" + this.User.Identity.Name;
                var dictionary = _configuration.GetSection("AzureAd").GetChildren()
                                                                     .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                                                     .ToDictionary(x => x.Key, x => x.Value);

                var accessToken = await _tokenHandler.GetAccessToken(dictionary);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await httpClient.GetAsync(_configuration["apiURL"] + queryString);
                var userBody = JsonConvert.DeserializeObject<User>(response.ToString());

                return View(userBody);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
