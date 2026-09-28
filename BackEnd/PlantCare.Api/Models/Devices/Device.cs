using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.Models.Devices;

public class Device
{
    public int DeviceId { get; set; }

    public int UserId { get; set; }

    public int WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    // Nullable: a device migrated from before this field existed has no known serial number yet.
    public string? SerialNumber { get; set; }

    public DeviceType DeviceType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastSeenAt { get; set; }

    public string ApiKeyHash { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public int BatteryLevel { get; set; } = 100;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public List<PlantDevice> PlantDevices { get; set; } = new();

    public ICollection<DeviceChannel> Channels { get; set; } = new List<DeviceChannel>();
}