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
            DeviceId = deviceEvent.DeviceId,
            ChannelId = deviceEvent.ChannelId,
            IrrigationZoneId = deviceEvent.IrrigationZoneId,
            DeviceCommandId = deviceEvent.DeviceCommandId,
            StartedAt = deviceEvent.StartedAt,
            DurationSeconds = deviceEvent.DurationSeconds,
            WaterAmountMl = deviceEvent.WaterAmountMl,
            WaterAmountSource = deviceEvent.WaterAmountSource,
            TriggerType = deviceEvent.TriggerType,
            Status = deviceEvent.Status
        };
    }
}