using PlantCare.Api.Models.Devices;
using PlantCare.Api.Models.Irrigation;
using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.Models.Automation;

public enum AlertType
{
    DeviceOffline,
    DeviceFault,
    LowBattery,
    PlantHealthIssue,
    IrrigationFailure,
    SensorReadingStale,
    Other
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public enum AlertStatus
{
    Open,
    Acknowledged,
    Resolved
}

// Linked to at most one concrete resource via a real FK per resource type (never a
// generic TargetId/TargetType pair) — null in all three means a workspace-level alert.
public class Alert
{
    public int AlertId { get; set; }

    public int WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public AlertType Type { get; set; }

    public AlertSeverity Severity { get; set; }

    public AlertStatus Status { get; set; } = AlertStatus.Open;

    public string Message { get; set; } = string.Empty;

    public int? PlantId { get; set; }

    public Plant? Plant { get; set; }

    public int? DeviceId { get; set; }

    public Device? Device { get; set; }

    public int? IrrigationZoneId { get; set; }

    public IrrigationZone? IrrigationZone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}
