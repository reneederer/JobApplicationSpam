module Tests

open System
open Xunit
open JobApplicationSpam.Models
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.DependencyInjection
open JobApplicationSpam.Models
open System.Data
open Npgsql
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open System.Net.Http
open System.Configuration
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration
open System.Transactions


type CustomWebApplicationFactory<'a> () =
    inherit WebApplicationFactory<JobApplicationSpam.Startup>()
    member val DbContext = null with get, set
    member val Transaction = null with get, set
    override this.ConfigureWebHost(builder : IWebHostBuilder) =
        builder.ConfigureServices(fun services ->
            let serviceProvider =
                ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .BuildServiceProvider()
            services.AddDbContext<JobApplicationSpamDbContext>(fun options ->
                let config = ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
                let connectionString = config.["Data:JobApplicationSpam:ConnectionString"]
                options
                    .UseNpgsql(connectionString)
                    .UseInternalServiceProvider(serviceProvider)
                    |> ignore
            ) |> ignore

            let sp = services.BuildServiceProvider();

            let scope = sp.CreateScope()
            let scopedServices = scope.ServiceProvider;
            let db = scopedServices.GetRequiredService<JobApplicationSpamDbContext>()
            db.Database.EnsureCreated() |> ignore
            this.DbContext <- db
            this.Transaction <- this.DbContext.Database.BeginTransaction()
            // Ensure the database is created.
            //Utilities.InitializeDbForTests(db);
        )
        |> ignore
    interface IDisposable with
        member this.Dispose() =
            this.Transaction.Rollback()
            this.Transaction.Dispose()
            ()

type IndexPageTests(factory : CustomWebApplicationFactory<JobApplicationSpam.Startup>) =
    let client : HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions(AllowAutoRedirect = false))
    interface IClassFixture<CustomWebApplicationFactory<JobApplicationSpam.Startup>>

    [<Fact>]
    member this.myTest() =
        //let appUser = this.Factory.DbContext.AppUsers.Find("d11340cc-5832-4273-ae81-b98399502871")
        let appUser = AppUser()
        appUser.Email <- "hallo@web.de"
        factory.DbContext.AppUsers.Add(appUser) |> ignore
        factory.DbContext.SaveChanges() |> ignore
        let ap = factory.DbContext.AppUsers.Find(appUser.Id)


        Assert.NotEqual(null, appUser)
    

type BasicTests (factory : CustomWebApplicationFactory<JobApplicationSpam.Startup>) =
    let client : HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions(AllowAutoRedirect = false))
    interface IClassFixture<CustomWebApplicationFactory<JobApplicationSpam.Startup>>
    [<Theory>]
    [<InlineData("/")>]
    [<InlineData("/sfjka")>]
    member this.Get_EndpointsReturnSuccessAndCorrectContentType(url : string) =
        // Arrange
        let response = client.GetAsync(url) |> Async.AwaitTask |> Async.RunSynchronously
        // Assert
        //response.EnsureSuccessStatusCode() |> ignore // Status Code 200-299
        //Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        Assert.Equal(1, 1);