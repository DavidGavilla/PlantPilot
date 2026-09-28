using PlantCare.Api.Models.Devices;
using PlantCare.Api.Models.Plants;

namespace PlantCare.Api.Models.Automation;

public class PhotoCaptureSchedule
{
    public int PhotoCaptureScheduleId { get; set; }

    public int CameraChannelId { get; set; }

    public DeviceChannel CameraChannel { get; set; } = null!;

    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;

    public int IntervalMinutes { get; set; }

    public DateTime NextRunAt { get; set; }

    public bool IsActive { get; set; } = true;
}
