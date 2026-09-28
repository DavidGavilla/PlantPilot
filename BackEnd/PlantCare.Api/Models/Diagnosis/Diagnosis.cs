using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.Models.Diagnoses;

public enum DiagnosisHealthStatus
{
    Healthy,
    Unhealthy,
    Indeterminate
}

public enum DiagnosisAnalysisStatus
{
    Pending,
    Completed,
    Failed
}

public class Diagnosis
{
    public int DiagnosisId { get; set; }

    public int PlantPhotoId { get; set; }

    public PlantPhoto PlantPhoto { get; set; } = null!;

    public DiagnosisHealthStatus HealthStatus { get; set; }

    public DiagnosisAnalysisStatus AnalysisStatus { get; set; } = DiagnosisAnalysisStatus.Pending;

    public decimal HealthProbability { get; set; }

    public string AiProvider { get; set; } = string.Empty;

    public string? AiModel { get; set; }

    public string? AiModelVersion { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string RawAiResponse { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DiagnosisProblem> Problems { get; set; } = new List<DiagnosisProblem>();

    public Schedule? Schedule { get; set; }
}