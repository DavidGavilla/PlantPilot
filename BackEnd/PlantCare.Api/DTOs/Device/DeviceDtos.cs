using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.DTOs.Devices;

public class CreateDeviceDto
{ 
    public string Name { get; set; } = string.Empty;

    public DeviceType DeviceType { get; set; }

    public string? Description { get; set; }


}

public class UpdateDeviceDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }
}

public class DeviceDto
{
    public int DeviceId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DeviceType DeviceType { get; set; }

    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public DateTime DateAdded { get; set; }

    public int BatteryLevel { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }
}