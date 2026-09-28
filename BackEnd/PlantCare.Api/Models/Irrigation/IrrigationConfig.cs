using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.Models.Irrigation;

public class IrrigationConfig
{
    public int IrrigationConfigId { get; set; }

    public int IrrigationZoneId { get; set; }

    public IrrigationZone IrrigationZone { get; set; } = null!;

    public int MoistureSensorChannelId { get; set; }

    public DeviceChannel MoistureSensorChannel { get; set; } = null!;

    // Desired configuration (server intent). Both thresholds are on the sensor's
    // calibrated scale (see DeviceChannel.CalibrationOffset/CalibrationScale).
    public double StartThreshold { get; set; }

    public double StopThreshold { get; set; }

    public int MaxDurationSeconds { get; set; }

    public int PauseBetweenIrrigationsSeconds { get; set; }

    public int MaxReadingAgeSeconds { get; set; }

    public bool IsActive { get; set; }

    public int Version { get; set; } = 1;

    // Configuration actually confirmed/applied by the device. Null until the device
    // acknowledges a version — do not assume the desired values are in effect until then.
    public double? ConfirmedStartThreshold { get; set; }

    public double? ConfirmedStopThreshold { get; set; }

    public int? ConfirmedMaxDurationSeconds { get; set; }

    public int? ConfirmedPauseBetweenIrrigationsSeconds { get; set; }

    public int? ConfirmedMaxReadingAgeSeconds { get; set; }

    public bool? ConfirmedIsActive { get; set; }

    public int? ConfirmedVersion { get; set; }

    public DateTime? ConfirmedAt { get; set; }
}
