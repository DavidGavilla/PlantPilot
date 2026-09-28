namespace PlantCare.Api.DTOs.Plants;

public class UpdatePlantDto
{
    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }

    public int? SoilMoistureLevel { get; set; }

    // Explicit null in the payload means "clear the plot".
    public int? PlotId { get; set; }

}
