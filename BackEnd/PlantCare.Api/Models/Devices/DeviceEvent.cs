namespace PlantCare.Api.Models.Devices;

public class DeviceEvent
{
    public int DeviceEventId { get; set; }

    public int PlantDeviceId { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }

    public TriggerType TriggerType { get; set; }

    public CommandStatus Status { get; set; } = CommandStatus.Pending;
}