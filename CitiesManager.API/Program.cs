using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CitiesManager.Infrastructure.DatabaseContext;
using CitiesManager.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Filters for content negotiation
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));

});

builder.Services.AddApiVersioning(config =>
{
    // Read API version from URL segment (e.g., /api/v1/cities)
    config.ApiVersionReader = new UrlSegmentApiVersionReader();
    // config.ApiVersionReader = new HeaderApiVersionReader("ver");
    // config.ApiVersionReader = new QueryStringApiVersionReader("v");

    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
}).AddApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV"; // e.g., v1, v2
        setup.SubstituteApiVersionInUrl = true;
    });

// Swagger setup
builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
    c.SwaggerDoc("v1", new OpenApiInfo(){Title = "Cities API v1", Version = "1.0"});
    c.SwaggerDoc("v2", new OpenApiInfo(){Title = "Cities API v2", Version = "2.0"});
});

var connectionString = builder.Configuration.GetConnectionString("Default") ??
                       throw new ArgumentNullException("CONNECTION STRING IS NULL!");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddEndpointsApiExplorer();

/*
    CORS: Which requests are allowed?
    CORS indicates that which domains can send request to our domain, like
    our API is hosted on api.cities.com, which domains are allowed to send requests.
    Here, we are allowing all domains to send requests to our API:
    
    builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(builder =>
       {
           builder.WithOrigins("*");
       });
   });
   

 */

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!)
            .WithHeaders("Authorization", "origin", "accept", "content-type")
            .WithMethods("GET", "POST", "PUT", "DELETE");
        // simply saying that only requests sent from localhost:4200 are allowed.

    });

    options.AddPolicy("4100Client", policy =>
    {
        policy
            .WithOrigins("http://localhost:4100")
            .WithHeaders("Authorization", "origin", "accept")
            .WithMethods("GET");
    });
});

var app = builder.Build();

await AutoMigrationHelper.ApplyPendingMigrationsAsync(app.Services);

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    o.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
});

app.UseRouting();
app.UseCors(); // if you don't add this you wouldn't be able to use CORS.
app.UseAuthorization();
app.MapControllers();
app.Run();

#region Notes / Explanations

/*
Controller Filters Notes:
- ProducesAttribute: Specifies the content type the action methods produce (here, JSON).
- ConsumesAttribute: Specifies the content type the action methods accept (here, JSON).
- You can also use these attributes directly on controllers or action methods:
  [Produces("content-type")]
  [Consumes("content-type")]
- AddXmlSerializerFormatters() enables serialization to XML if [Produces("application/xml")] is used.

API Versioning Notes:
- Three main ways to read API version from requests:
  1. UrlSegmentApiVersionReader (used here): version from URL path (e.g., /api/v1/cities)
  2. HeaderApiVersionReader: version from a custom header (e.g., Headers: api-version: 1.0)
     - You can rename the header: new HeaderApiVersionReader("ver")
  3. QueryStringApiVersionReader: version from query string (e.g., ?api-version=1.0)
     - You can rename the query param: new QueryStringApiVersionReader("ver")

Swagger Notes:
- AddEndpointsApiExplorer(): enables Swagger to read all action methods/endpoints.
- AddSwaggerGen(): generates Swagger documents; can include XML comments and multiple versions.
- UseSwagger(): exposes the swagger.json endpoint.
- UseSwaggerUI(): provides a web interface for swagger documentation.
*/

#endregion
