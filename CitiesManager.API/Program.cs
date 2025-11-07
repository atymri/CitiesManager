using System.Text;
using CitiesManager.API.Extensions;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using CitiesManager.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("/logs/logs.txt"
        , outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        , rollingInterval: RollingInterval.Day)
    .WriteTo.Seq(serverUrl: "http://localhost:5341/", period: TimeSpan.FromSeconds(2))
    .CreateLogger();


builder.Host.UseSerilog();

var app = builder.Build();

await AutoMigrationHelper.ApplyPendingMigrationsAsync(app.Services);

app.UseHsts();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    o.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");

    o.OAuthClientId("swagger-ui");
    o.OAuthAppName("Cities Manager API - Swagger");
    o.OAuthUsePkce();
});

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();