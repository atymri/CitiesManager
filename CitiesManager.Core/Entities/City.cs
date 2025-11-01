using System.ComponentModel.DataAnnotations;

namespace CitiesManager.Core.Entities;

public class City
{
    [Key]
    public Guid CityID { get; set; }
    [Required]
    [StringLength(25)]
    public string? CityName { get; set; }

    public DateTime DateAdded { get; private set; } = DateTime.UtcNow;
}