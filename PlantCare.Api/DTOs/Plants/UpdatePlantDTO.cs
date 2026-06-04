namespace PlantCare.Api.DTOs.Plants;

public class UpdatePlantDto
{
    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }
}