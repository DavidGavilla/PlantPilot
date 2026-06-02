using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.Models.Diagnoses;

public class Diagnosis
{
    public int DiagnosisId { get; set; }

    public int PlantPhotoId { get; set; }

    public PlantPhoto PlantPhoto { get; set; } = null!;

    public bool IsHealthy { get; set; }

    public decimal HealthProbability { get; set; }

    public string AiProvider { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string RawAiResponse { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DiagnosisProblem> Problems { get; set; } = new List<DiagnosisProblem>();

    public Schedule? Schedule { get; set; }
}