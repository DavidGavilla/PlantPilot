using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.DTOs.Devices;

public class DeviceEventDto
{
    public int DeviceEventId { get; set; }

    public int DeviceId { get; set; }

    public int? ChannelId { get; set; }

    public int? IrrigationZoneId { get; set; }

    public int? DeviceCommandId { get; set; }

    public DateTime StartedAt { get; set; }

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }

    public WaterAmountSource WaterAmountSource { get; set; }

    public TriggerType TriggerType { get; set; }

    public CommandStatus Status { get; set; }
}