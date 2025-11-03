using CitiesManager.API.Extensions;
using CitiesManager.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

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
app.UseAuthentication();
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
