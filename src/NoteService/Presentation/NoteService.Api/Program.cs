using Demo1.Helper.Exceptions;
using Microsoft.OpenApi.Models;
using NoteService.Application.Models;
using NoteService.Application.Registration;
using NoteService.Infrastructure.Registration;
using NoteService.Persistence.Registration;
using NoteService.Persistence.Seeds;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var configuration = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration/Settings"))
    .AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var appSettings = new AppSettings();
configuration.Bind(nameof(AppSettings), appSettings);

builder.Services.AddSingleton(appSettings);
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices().AddInfrastructureServices(configuration).AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.UsePersistence();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ExceptionMiddleware>();
await app.Services.InitializeDatabase();
app.Run();
