using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.Models.Plants;

public class Plant
{
    public int PlantId { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public ICollection<PlantPhoto> Photos { get; set; } = new List<PlantPhoto>();

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public int? SoilMoistureLevel { get; set; }    
    public List<PlantDevice> PlantDevices { get; set; } = new();
}