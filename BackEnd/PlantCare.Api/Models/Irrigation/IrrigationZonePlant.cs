using PlantCare.Api.Models.Plants;

namespace PlantCare.Api.Models.Irrigation;

public class IrrigationZonePlant
{
    public int IrrigationZonePlantId { get; set; }

    public int IrrigationZoneId { get; set; }

    public IrrigationZone IrrigationZone { get; set; } = null!;

    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;

    // Denormalized from IrrigationZone/Plant (both must already belong to the same workspace) so
    // the composite FKs in AppDbContext can enforce it in the database — see
    // docs/architecture/workspace-data-model.md.
    public int WorkspaceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
