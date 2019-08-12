using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Handler.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ModernAuth_UI.Models;
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
            string accessToken = "";
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var dictionary = _configuration.GetSection("AzureAd").GetChildren()
                                                                     .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                                                     .ToDictionary(x => x.Key, x => x.Value);

                try
                {
                    accessToken = await _tokenHandler.GetAccessTokenSilently(dictionary);
                }
                catch(Exception e)
                {
                    return RedirectToAction("SignIn", "Account");
                }

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.GetAsync(_configuration["apiURL"] + this.User.Identity.Name);
                var responseContent = await response.Content.ReadAsStringAsync();
                var userBody = JsonConvert.DeserializeObject<User>(responseContent);

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
