using PlantCare.Api.Models.Devices;
using PlantCare.Api.Models.Farms;
using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.Models.Irrigation;

public class IrrigationZone
{
    public int IrrigationZoneId { get; set; }

    public int WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int? PlotId { get; set; }

    public Plot? Plot { get; set; }

    public int ValveChannelId { get; set; }

    public DeviceChannel ValveChannel { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<IrrigationZonePlant> Plants { get; set; } = new List<IrrigationZonePlant>();

    public IrrigationConfig? Config { get; set; }
}
