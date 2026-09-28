namespace PlantCare.Api.Models.Devices;

public enum DeviceChannelType
{
    Sensor,
    Camera,
    ValveOutput,
    PumpOutput
}

public class DeviceChannel
{
    public int DeviceChannelId { get; set; }

    public int DeviceId { get; set; }

    public Device Device { get; set; } = null!;

    public DeviceChannelType ChannelType { get; set; }

    public string Label { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Simple linear calibration: calibratedValue = rawValue * CalibrationScale + CalibrationOffset.
    // Null means no calibration has been applied yet (use the raw reading as-is).
    public double? CalibrationOffset { get; set; }

    public double? CalibrationScale { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
