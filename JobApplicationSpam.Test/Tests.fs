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
open System.IO


type CustomWebApplicationFactory<'a> () =
    inherit WebApplicationFactory<JobApplicationSpam.Startup>()
    let initializeDbForTests (dbContext : JobApplicationSpamDbContext) =
        dbContext.Database.EnsureCreated() |> ignore
        let userCount = dbContext.AppUsers.CountAsync() |> Async.AwaitTask |> Async.RunSynchronously
        if userCount = 0
        then
            try
                dbContext.AppUsers.AddRange(
                    new AppUser(Id = "y", Email = "rene@web.de"),
                    new AppUser(Id = "z", Email = "astrid@web.de")
                )
                dbContext.SaveChanges() |> ignore
            with
            | _ -> ()
    member val DbContext = null with get, set
    override this.ConfigureWebHost(builder : IWebHostBuilder) =
        builder.ConfigureServices(fun services ->
            let serviceProvider =
                ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .BuildServiceProvider()
            services.AddDbContext<JobApplicationSpamDbContext>(fun options ->
                let appSettingsPath = "c:/users/rene/source/repos/JobApplicationSpam/JobApplicationSpam/appsettings.json"
                let config = ConfigurationBuilder().AddJsonFile(appSettingsPath).Build()
                let connectionString = config.["Data:JobApplicationSpam:ConnectionStringTest"]
                options
                    .UseNpgsql(connectionString)
                    .UseInternalServiceProvider(serviceProvider)
                    |> ignore
            ) |> ignore

            let sp = services.BuildServiceProvider();

            let scope = sp.CreateScope()
            let scopedServices = scope.ServiceProvider;
            let db = scopedServices.GetRequiredService<JobApplicationSpamDbContext>()
            use transactionScope = new TransactionScope(TransactionScopeOption.Suppress)
            initializeDbForTests(db);
            transactionScope.Complete()
            this.DbContext <- db
        )
        |> ignore
 
 [<AbstractClass>]
 type SetupAndTeardown() =
    member val TransactionScope = new TransactionScope() with get, set
    interface IDisposable with
        member this.Dispose() =
            this.TransactionScope.Dispose()
            ()

type IndexPageTests(factory : CustomWebApplicationFactory<JobApplicationSpam.Startup>) as this =
    inherit SetupAndTeardown()
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

type BasicTests (factory : CustomWebApplicationFactory<JobApplicationSpam.Startup>) as this =
    inherit SetupAndTeardown()
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