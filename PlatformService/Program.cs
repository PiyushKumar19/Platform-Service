using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Http;

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
    builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(deployedServerCN));
}
else
{
    Console.WriteLine("--> Using local SqlServer Db");

    builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultString")));
}


builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

Console.WriteLine($"--> CommandService Endpoint {builder.Configuration["CommandServiceIIS"]}");


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();
PrepDb.PrepPopulation(app, app.Environment.IsProduction());

app.Run();
