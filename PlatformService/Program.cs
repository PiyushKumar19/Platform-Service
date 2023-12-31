using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");
var dockerCN = $"server={dbHost}; database={dbName}; User Id=sa; password = {dbPassword}; TrustServerCertificate=True";
var localCN = builder.Configuration.GetConnectionString("DefaultString");
var deployedLocalCN = builder.Configuration.GetConnectionString("LocalDeployedString");
var deployedServerCN = builder.Configuration.GetConnectionString("DeployedConn");


builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
if (builder.Environment.IsProduction())
{
    builder.WebHost.UseUrls("http://*.80");
    builder.WebHost.UseIISIntegration();

    Console.WriteLine("--> Using deployed SqlServer Db");
    Console.WriteLine($"--> CommandService Endpoint {builder.Configuration["CommandService"]}");

    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(deployedServerCN));
}
else
{
    Console.WriteLine("--> Using local SqlServer Db");

    Console.WriteLine($"--> CommandService Endpoint {builder.Configuration["CommandService"]}");

    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultString")));
}


builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddGrpc();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
            {
                var grpcPort = 5155;
                var httpPort = 5121; // just a randowm number

                if (options.ApplicationServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
                {
                    options.Listen(IPAddress.Any, httpPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1;
                    });

                    options.Listen(IPAddress.Any, grpcPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                }
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

#pragma warning disable ASP0014
app.UseEndpoints(endpoint =>
{
    endpoint.MapGrpcService<GrpcPlatformService>();
    endpoint.MapGet("/protos/platforms.proto", async context =>
    {
        await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
    });
});
#pragma warning restore ASP0014


PrepDb.PrepPopulation(app, app.Environment.IsProduction());

app.Run();
