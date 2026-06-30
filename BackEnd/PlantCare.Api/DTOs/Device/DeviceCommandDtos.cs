using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.DTOs.Devices;

public class CreateDeviceCommandDto
{
    public int PlantDeviceId { get; set; }

    public DeviceCommandType CommandType { get; set; }

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }
}

public class DeviceCommandDto
{
    public int DeviceCommandId { get; set; }

    public int PlantDeviceId { get; set; }

    public DeviceCommandType CommandType { get; set; }

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }

    public CommandStatus Status { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? CompletedDate { get; set; }
}