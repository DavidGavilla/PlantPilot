namespace PlantCare.Api.DTOs.Plants;

public class CreatePlantDto
{
    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }

    public int? SoilMoistureLevel { get; set; }

    public int? PlotId { get; set; }

}
