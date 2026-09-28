using PlantCare.Api.Models.Diagnoses;

namespace PlantCare.Api.DTOs.Diagnoses;

public class CreateDiagnosisDto
{
    public int PlantPhotoId { get; set; }

    public DiagnosisHealthStatus HealthStatus { get; set; }

    public DiagnosisAnalysisStatus AnalysisStatus { get; set; } = DiagnosisAnalysisStatus.Pending;

    public decimal HealthProbability { get; set; }

    public string AiProvider { get; set; } = string.Empty;

    public string? AiModel { get; set; }

    public string? AiModelVersion { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string RawAiResponse { get; set; } = string.Empty;

    public List<CreateDiagnosisProblemDto> Problems { get; set; } = new();
}