using NetTopologySuite.Geometries;

namespace PlantCare.Api.Models.Farms;

public class Plot
{
    public int PlotId { get; set; }

    public int FarmId { get; set; }

    public Farm Farm { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public Geometry? Boundary { get; set; }

    public string? CropType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
