using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WiredBrain.CustomerPortal.Web.Data;
using WiredBrain.CustomerPortal.Web.Repositories;

namespace WiredBrain.CustomerPortal.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            this._config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CustomerPortalDbContext>(options =>
                options.UseSqlServer(_config.GetConnectionString("CustomerDb")));

            services.AddScoped<IDbConnection>(_ => new SqlConnection(_config.GetConnectionString("CustomerDb")));
            services.AddTransient<IClaimsTransformation, CustomClaimsTransformer>();

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddSingleton(_config);
            
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    options.Authority = "https://localhost:44347/";

                    options.ClientId = "wb";
                    options.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";

                    options.ResponseType = "code";
                    options.UsePkce = true;
                    
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("wiredbrain");
                    options.Scope.Add("wiredapi");

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {

                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                    options.ClaimActions.MapAllExcept("nbf", "exp", "aud", "nonce", "iat", "c_hash");

                    // https://docs.microsoft.com/en-us/dotnet/api/
                    // microsoft.aspnetcore.authentication.openidconnect.openidconnectevents
                    options.Events.OnTicketReceived = e =>
                    {
                        Log.Information("Login successfully completed for {UserName}." , 
                            e.Principal.Identity.Name);
                        return Task.CompletedTask;
                    };
                });


            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler("/Home/Error");

            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        private ClaimsPrincipal TransformClaims(ClaimsPrincipal argPrincipal)
        {
            
            return argPrincipal;
        }
    }
}
