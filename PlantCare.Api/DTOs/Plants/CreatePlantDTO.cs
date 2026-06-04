namespace PlantCare.Api.DTOs.Plants;

public class CreatePlantDto
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }
}