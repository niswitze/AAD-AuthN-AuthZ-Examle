using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Handler.Auth;

namespace ModernAuth_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //check for UseMSAL value will determine if ADAL or MSAL will be used for managing access tokens
            if (Convert.ToBoolean(Configuration["UseMSAL"]))
            {
                //custom service for authentication and authorization. Not added by configuration wizard
                services.AddSingleton<ITokenHandler<IDictionary<string, string>>,
                      MSALTokenHandler<IDictionary<string, string>>>();
            }
            else
            {
                //custom service for authentication and authorization. Not added by configuration wizard
                services.AddSingleton<ITokenHandler<IDictionary<string, string>>,
                      ADALTokenHandler<IDictionary<string, string>>>();
            }


            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddAzureAdBearer(options => Configuration.Bind("AzureAd", options));

            services.AddMvc();
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();        
            app.UseMvc();
        }
    }
}
