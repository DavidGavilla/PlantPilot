namespace PlantCare.Api.Models.Devices;

public class DeviceCommand
{
    public int DeviceCommandId { get; set; }

    public int PlantDeviceId { get; set; }

    public DeviceCommandType CommandType { get; set; }

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }

    public CommandStatus Status { get; set; } = CommandStatus.Pending;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedDate { get; set; }
}