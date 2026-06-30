using PlantCare.Api.DTOs.Devices;
using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.Mappers.Devices;

public static class DeviceEventMapper
{
    public static DeviceEventDto ToDto(this DeviceEvent deviceEvent)
    {
        return new DeviceEventDto
        {
            DeviceEventId = deviceEvent.DeviceEventId,
            PlantDeviceId = deviceEvent.PlantDeviceId,
            StartedAt = deviceEvent.StartedAt,
            DurationSeconds = deviceEvent.DurationSeconds,
            WaterAmountMl = deviceEvent.WaterAmountMl,
            TriggerType = deviceEvent.TriggerType,
            Status = deviceEvent.Status
        };
    }
}