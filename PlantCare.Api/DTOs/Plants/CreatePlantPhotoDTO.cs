namespace PlantCare.Api.DTOs.Plants;

public class PlantPhotoDto
{
    public int PlantPhotoId { get; set; }
    public int PlantId { get; set; }

    public string PhotoUrl { get; set; } = string.Empty;

    public DateTime DateAdded { get; set; }

}