using PlantCare.Api.Models.Plants;

public class CreatePlantPhotoDto
{
    public int PlantPhotoId { get; set; }

    public int PlantId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public PlantPhotoSource Source { get; set; }

    public int? CameraChannelId { get; set; }

    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}