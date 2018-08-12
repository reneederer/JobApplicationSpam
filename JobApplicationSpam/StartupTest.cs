/*using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobApplicationSpam.Models;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JobApplicationSpam
{
    public class StartupTest
    {
        public static IConfiguration Configuration { get; set;  }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            services
                .AddDbContext<JobApplicationSpamDbContext>(options =>
                    options
                        .UseNpgsql(Configuration["Data:JobApplicationSpam:ConnectionString"])
                )
                .AddIdentity<AppUser, IdentityRole>(options =>
                    {
                        options.Password.RequiredLength = 1;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireDigit = false;
                        options.User.RequireUniqueEmail = false;
                    }
                ).AddEntityFrameworkStores<JobApplicationSpamDbContext>().AddDefaultTokenProviders();
            services.AddSession();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseStatusCodePages()
                .UseDeveloperExceptionPage()
                .UseStaticFiles()
                .UseAuthentication()
                .UseSession();
            app.Use(next => async context =>
            {
                await SignInIntegrationTestUser(context);
                await next.Invoke(context);
            });
            app.UseMvcWithDefaultRoute();
        }


        public async Task SignInIntegrationTestUser(HttpContext context)
        {
            var integrationTestsUserHeader = context.Request.Headers["IntegrationTestLogin"];
            if (integrationTestsUserHeader.Count > 0)
            {
                var userName = integrationTestsUserHeader[0];
                var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(userName);
                if (user == null)
                {
                    return;
                }
                var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
                var userIdentity = await signInManager.CreateUserPrincipalAsync(user);
                context.User = userIdentity;
            }
        }








    }
}
*/
