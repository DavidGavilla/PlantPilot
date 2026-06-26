namespace PlantCare.Api.DTOs.Plants;

public class PlantDto
{
    public int PlantId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }

    public DateTime DateAdded { get; set; }

    public int? SoilMoistureLevel { get; set; }

}