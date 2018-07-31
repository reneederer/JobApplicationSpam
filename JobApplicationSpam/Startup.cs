using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobApplicationSpam.Models;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;


namespace JobApplicationSpam
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<JobApplicationSpamDbContext>(options =>
                    options.UseNpgsql(Configuration["Data:JobApplicationSpam:ConnectionString"]))
                .AddIdentity<AppUser, IdentityRole>(options =>
                    {
                        options.Password.RequiredLength = 0;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireDigit = false;
                        options.User.RequireUniqueEmail = true;
                    }
                )
                .AddEntityFrameworkStores<JobApplicationSpamDbContext>().AddDefaultTokenProviders();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app
                .UseStatusCodePages()
                .UseDeveloperExceptionPage()
                .UseStaticFiles()
                .UseAuthentication()
                .UseMvcWithDefaultRoute();
        }
    }
}
