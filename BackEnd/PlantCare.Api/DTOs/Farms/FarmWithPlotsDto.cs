namespace PlantCare.Api.DTOs.Farms;

public class FarmWithPlotsDto
{
    public int FarmId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<PlotOptionDto> Plots { get; set; } = new();
}

public class PlotOptionDto
{
    public int PlotId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? CropType { get; set; }
}
