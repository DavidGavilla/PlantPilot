using PlantCare.Api.Models.Diagnoses;

namespace PlantCare.Api.Models.Plants;

public class PlantPhoto
{
    public int PlantPhotoId { get; set; }

    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public Diagnosis? Diagnosis { get; set; }


}