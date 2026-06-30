using PlantCare.Api.DTOs.Devices;
using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.Mappers.Devices;

public static class DeviceReadingMapper
{
    public static DeviceReadingDto ToDto(this DeviceReading reading)
    {
        return new DeviceReadingDto
        {
            DeviceReadingId = reading.DeviceReadingId,
            PlantDeviceId = reading.PlantDeviceId,
            CreatedAt = reading.CreatedAt,
            ReadingType = reading.ReadingType,
            Value = reading.Value,
            Unit = reading.Unit
        };
    }

    public static DeviceReading ToEntity(this CreateDeviceReadingDto dto)
    {
        return new DeviceReading
        {
            PlantDeviceId = dto.PlantDeviceId,
            ReadingType = dto.ReadingType,
            Value = dto.Value,
            Unit = dto.Unit
        };
    }
}