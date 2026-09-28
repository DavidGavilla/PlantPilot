using PlantCare.Api.Models.Irrigation;

namespace PlantCare.Api.Models.Devices;

public enum WaterAmountSource
{
    Unknown,
    Measured,
    Estimated
}

public class DeviceEvent
{
    public int DeviceEventId { get; set; }

    public int DeviceId { get; set; }

    public Device Device { get; set; } = null!;

    public int? ChannelId { get; set; }

    public DeviceChannel? Channel { get; set; }

    public int? IrrigationZoneId { get; set; }

    public IrrigationZone? IrrigationZone { get; set; }

    public int? DeviceCommandId { get; set; }

    public DeviceCommand? DeviceCommand { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }

    public WaterAmountSource WaterAmountSource { get; set; } = WaterAmountSource.Unknown;

    public TriggerType TriggerType { get; set; }

    public CommandStatus Status { get; set; } = CommandStatus.Pending;
}