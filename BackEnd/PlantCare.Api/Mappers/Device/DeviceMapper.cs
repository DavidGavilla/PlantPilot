using PlantCare.Api.DTOs.Devices;
using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.Mappers.Devices;

public static class DeviceMapper
{
    public static DeviceDto ToDto(this Device device)
    {
        return new DeviceDto
        {
            DeviceId = device.DeviceId,
            UserId = device.UserId,
            Name = device.Name,
            DeviceType = device.DeviceType,
            IsActive = device.IsActive,
            Description = device.Description,
            DateAdded = device.DateAdded,
            BatteryLevel = device.BatteryLevel,
            Latitude = device.Latitude,
            Longitude = device.Longitude
        };
    }

    public static Device ToEntity(this CreateDeviceDto dto)
    {
        return new Device
        {
            Name = dto.Name,
            DeviceType = dto.DeviceType,
            Description = dto.Description
        };
    }

    public static void UpdateEntity(this UpdateDeviceDto dto, Device device)
    {
        device.Name = dto.Name;
        device.Description = dto.Description;
        device.IsActive = dto.IsActive;
        device.Latitude = dto.Latitude;
        device.Longitude = dto.Longitude;
    }
    
}