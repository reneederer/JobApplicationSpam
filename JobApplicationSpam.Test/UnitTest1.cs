using System;
using Xunit;
using JobApplicationSpam.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Npgsql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Transactions;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore;
using Moq;
using System.Security.Claims;

namespace JobApplicationSpam
{
    public class HomeTest : IDisposable
    {
        private TestServer Server { get; }
        private HttpClient Client { get; set; }

        private void login(string loggedInUserId)
        {
            Client.DefaultRequestHeaders.Add("IntegrationTestLogin", loggedInUserId);
        }

        public HomeTest()
        {
            var builder =
                new WebHostBuilder()
                  .UseContentRoot(@"C:\users\rene\source\repos\JobApplicationSpam\JobApplicationSpam")
                  .UseEnvironment("Development")
                  .UseStartup<JobApplicationSpam.Startup>()
                  .UseApplicationInsights();
            Server = new TestServer(builder);
            Client = Server.CreateClient();
        }

        public void Dispose()
        {
            Client.Dispose();
            Server.Dispose();
        }

        [Fact]
        public async Task Index_Get_ReturnsIndexHtmlPage()
        {
            // Act
            var response = await Client.GetAsync("/Home/Mimi");
            //UserManager.GetUserAsync(HttpContext.User);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("<title>Bewerbungsspam</title>", responseString);
        }
    }
}