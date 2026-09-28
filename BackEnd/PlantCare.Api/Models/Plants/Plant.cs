using PlantCare.Api.Models.Farms;
using PlantCare.Api.Models.Schedules;
using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.Models.Plants;

public class Plant
{
    public int PlantId { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public int? PlotId { get; set; }

    public Plot? Plot { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ScientificName { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public ICollection<PlantPhoto> Photos { get; set; } = new List<PlantPhoto>();

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public int? SoilMoistureLevel { get; set; }
    public List<PlantDevice> PlantDevices { get; set; } = new();

    // Archive/soft-delete flag: several dependent FKs (Alert, IrrigationZonePlant, diagnosis-less Schedule) are Restrict and block a hard delete, so archiving is the safe default.
    public bool IsArchived { get; set; } = false;

    public DateTime? ArchivedAt { get; set; }
}