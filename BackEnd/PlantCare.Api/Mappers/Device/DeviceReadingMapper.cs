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
            ChannelId = reading.ChannelId,
            ReadingType = reading.ReadingType,
            Value = reading.Value,
            Unit = reading.Unit,
            MeasuredAt = reading.MeasuredAt,
            ReceivedAt = reading.ReceivedAt
        };
    }

    public static DeviceReading ToEntity(this CreateDeviceReadingDto dto)
    {
        return new DeviceReading
        {
            ChannelId = dto.ChannelId,
            ReadingType = dto.ReadingType,
            Value = dto.Value,
            Unit = dto.Unit,
            MeasuredAt = dto.MeasuredAt
        };
    }
}