using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using CitiesManager.Core.Entities;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Cors;

namespace CitiesManager.API.Controllers.v1;

/// <summary>
/// Controller for managing city entities.
/// </summary>
[ApiVersion("1.0")] // we have enabled UrlSegmentApiVersionReader from Program.cs
//[EnableCors("4100Client")]
public class CitiesController(ApplicationDbContext context) : BaseController
{
    private readonly ApplicationDbContext _context = context ?? throw new NotImplementedException(nameof(context));
    /// <summary>
    /// Retrieves all cities from the database, ordered by city name.
    /// </summary>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a list of all <see cref="City"/> objects.
    /// </returns>
    // GET: api/Cities
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    //[Produces("application/xml")]
    public async Task<ActionResult<IEnumerable<City>>> GetCities()
    {
        return await context.Cities
            .OrderBy(city => city.CityName)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific c ity by its unique identifier.
    /// </summary>
    /// <param name="cityId">The unique identifier of the city.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the <see cref="City"/> object if found; otherwise, a Problem response with status 404.
    /// </returns>
    // GET: api/Cities/{cityId}
    [HttpGet("{cityId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
    public async Task<ActionResult<City>> GetCity(Guid cityId)
    {
        var city = await context.Cities.FindAsync(cityId);

        if (city == null)
        {
            return Problem(detail: "No city was found with given id",
                statusCode: StatusCodes.Status404NotFound,
                title: "Search Cities");
        }

        return city;
    }

    /// <summary>
    /// Updates an existing city with new values.
    /// </summary>
    /// <param name="cityId">The unique identifier of the city to update.</param>
    /// <param name="city">The <see cref="City"/> object containing updated values.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the update operation:
    /// <list type="bullet">
    /// <item>204 No Content if successful.</item>
    /// <item>400 Bad Request if the IDs do not match.</item>
    /// <item>404 Not Found if the city does not exist.</item>
    /// </list>
    /// </returns>
    // PUT: api/Cities/{cityId}
    [HttpPut("{cityId}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PutCity(Guid cityId, City city)
    {
        //if (!ModelState.IsValid)
        //    return ValidationProblem(ModelState); // This is automatically done by aspnetcore, you don't need to do it.

        if (cityId != city.CityID)
        {
            return Problem(detail: "Given CityID and the objects ID dont match",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Update City");
        }

        var storedCity = await context.Cities.FirstOrDefaultAsync(city => city.CityID == cityId);
        if (storedCity == null)
        {
            return Problem(detail: "No Cities were found with given CityID",
                statusCode: StatusCodes.Status404NotFound,
                title: "Update City");
        }

        storedCity.CityName = city.CityName;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CityExists(cityId))
            {
                return Problem(detail: "No cities were found with the given CityID",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Update City");
            }

            throw;
        }

        return NoContent(); // HTTP 204
    }

    /// <summary>
    /// Creates a new city in the database.
    /// </summary>
    /// <param name="city">The <see cref="City"/> object to create.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the created <see cref="City"/> object and a 201 Created response.
    /// </returns>
    // POST: api/Cities
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<City>> PostCity(City city)
    {
        context.Cities.Add(city);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCity), new { cityID = city.CityID }, city); // HTTP 201
    }

    /// <summary>
    /// Deletes a city from the database by its unique identifier.
    /// </summary>
    /// <param name="cityId">The unique identifier of the city to delete.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the delete operation:
    /// <list type="bullet">
    /// <item>204 No Content if successful.</item>
    /// <item>404 Not Found if the city does not exist.</item>
    /// </list>
    /// </returns>
    // DELETE: api/Cities/{cityId}
    [HttpDelete("{cityId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCity(Guid cityId)
    {
        var city = await context.Cities.FindAsync(cityId);
        if (city == null)
        {
            return Problem(detail: "No cities were found with the given CityID",
                statusCode: StatusCodes.Status404NotFound,
                title: "Delete City");
        }

        context.Cities.Remove(city);
        await context.SaveChangesAsync();

        return NoContent(); // HTTP 204
    }

    /// <summary>
    /// Checks if a city exists in the database by its unique identifier.
    /// </summary>
    /// <param name="cityId">The unique identifier of the city.</param>
    /// <returns>
    /// <c>true</c> if the city exists; otherwise, <c>false</c>.
    /// </returns>
    private bool CityExists(Guid cityId)
    {
        return context.Cities.Any(e => e.CityID == cityId);
    }
}
