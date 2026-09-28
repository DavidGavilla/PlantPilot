namespace PlantCare.Api.DTOs.Plants;

public class PlantDto
{
    public int PlantId { get; set; }

    public int UserId { get; set; }

    public int WorkspaceId { get; set; }

    public int? PlotId { get; set; }

    // Read-only display convenience, populated via a join in the service — never settable from a
    // create/update DTO.
    public string? PlotName { get; set; }

    public int? FarmId { get; set; }

    public string? FarmName { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }

    public DateTime DateAdded { get; set; }

    public int? SoilMoistureLevel { get; set; }

    public bool IsArchived { get; set; }

    public DateTime? ArchivedAt { get; set; }

}
