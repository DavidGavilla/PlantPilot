using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.DTOs.Devices;

public class DeviceEventDto
{
    public int DeviceEventId { get; set; }

    public int PlantDeviceId { get; set; }

    public DateTime StartedAt { get; set; }

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }

    public TriggerType TriggerType { get; set; }

    public CommandStatus Status { get; set; }
}