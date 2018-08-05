using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobApplicationSpam.Models;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Text;
using Microsoft.Extensions.Logging;

namespace JobApplicationSpam
{
    public class Startup
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
                    options.UseNpgsql(Configuration["Data:JobApplicationSpam:ConnectionString"]))
                .AddIdentity<AppUser, IdentityRole>(options =>
                    {
                        options.Password.RequiredLength = 1;
                        options.Password.RequireNonAlphanumeric = false;
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
                .UseSession()
                .UseMvcWithDefaultRoute();
        }
    }
}
