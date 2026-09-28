using PlantCare.Api.Models.Diagnoses;

namespace PlantCare.Api.DTOs.Diagnoses;

public class DiagnosisDto
{
    public int DiagnosisId { get; set; }

    public int PlantPhotoId { get; set; }

    public DiagnosisHealthStatus HealthStatus { get; set; }

    public DiagnosisAnalysisStatus AnalysisStatus { get; set; }

    public decimal HealthProbability { get; set; }

    public string AiProvider { get; set; } = string.Empty;

    public string? AiModel { get; set; }

    public string? AiModelVersion { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<DiagnosisProblemDto> Problems { get; set; } = new();
}