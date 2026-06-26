namespace PlantCare.Api.Models.Devices;

public class Device
{
    public int DeviceId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DeviceType DeviceType { get; set; }

    public bool IsActive { get; set; } = true;

    public string ApiKeyHash { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public int BatteryLevel { get; set; } = 100;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public List<PlantDevice> PlantDevices { get; set; } = new();
}