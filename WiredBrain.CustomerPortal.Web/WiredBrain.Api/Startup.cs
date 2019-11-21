using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WiredBrain.Api.Data;
using WiredBrain.Api.Middleware;
using WiredBrain.Api.Repositories;

namespace WiredBrain.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CustomerPortalDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("CustomerDb")));

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:44347/";
                    options.Audience = "wiredapi";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                    //https://docs.microsoft.com/en-us/dotnet/api/
                    // microsoft.aspnetcore.authentication.jwtbearer.jwtbearerevents
                    options.Events = new JwtBearerEvents
                    {
                        OnForbidden = e =>
                        {
                            Log.Warning("API access was forbidden!");
                            return Task.FromResult(e);
                        },
                        OnAuthenticationFailed = e =>
                        {
                            Log.Warning(e.Exception, "Authentication Failed!");
                            return Task.FromResult(e);
                        }
                    };
                });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseApiExceptionHandler();  // custom middleware

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
