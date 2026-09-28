using PlantCare.Api.Models.Devices;
using PlantCare.Api.Models.Diagnoses;

namespace PlantCare.Api.Models.Plants;

public enum PlantPhotoSource
{
    Mobile,
    Camera
}

public class PlantPhoto
{
    public int PlantPhotoId { get; set; }

    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    public PlantPhotoSource Source { get; set; }

    public int? CameraChannelId { get; set; }

    public DeviceChannel? CameraChannel { get; set; }

    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Diagnosis? Diagnosis { get; set; }
}