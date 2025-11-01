using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Asp.Versioning;
using CitiesManager.Core.Entities;
using CitiesManager.Infrastructure.DatabaseContext;

namespace CitiesManager.API.Controllers.v2;

/// <summary>
/// Controller for managing city entities.
/// </summary>
///
[ApiVersion("2.0")] // we have enabled ApiSegmentUrlVersionReader from Progrma.cs
public class CitiesController(ApplicationDbContext context) : BaseController
{
    private readonly ApplicationDbContext _context = context ?? throw new NotImplementedException(nameof(context));
    /// <summary>
    /// Retrieves all cities from the database, ordered by city name.
    /// </summary>
    /// <returns>
    /// a list of all <see cref="City"/> name objects.
    /// </returns>
    // GET: api/Cities
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    //[Produces("application/xml")]
    public async Task<List<string?>> GetCities()
        => await context.Cities
                .OrderBy(city => city.CityName)
                .Select(city => city.CityName)
                .ToListAsync();
        

}
