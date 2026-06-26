public class CreatePlantPhotoDto
{
    public int PlantPhotoId { get; set; }

    public int PlantId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;


}