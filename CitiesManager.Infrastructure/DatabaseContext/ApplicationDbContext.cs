using CitiesManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.Infrastructure.DatabaseContext;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }
    public ApplicationDbContext() { }

    public virtual DbSet<City> Cities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var cities = new List<City>()
        {
            new City()
            {
                CityID = Guid.Parse("23b46d49-25ec-4b04-be3a-ea73f5a3d5d1"),
                CityName = "Mumbai"
            },
            new City()
            {
                CityID = Guid.Parse("d0c788c0-6f24-4084-974d-79ebdcf352b7"),
                CityName = "London"
            },
            new City()
            {
                CityID = Guid.Parse("f70d6086-17c4-4f49-9bcb-41a3382ea991"),
                CityName = "Tehran"
            }

        };

        foreach (var city in cities)
            modelBuilder.Entity<City>().HasData(city);
    }
}