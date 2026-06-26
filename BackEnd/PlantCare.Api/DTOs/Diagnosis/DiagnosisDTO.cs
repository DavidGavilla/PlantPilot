namespace PlantCare.Api.DTOs.Diagnoses;

public class DiagnosisDto
{
    public int DiagnosisId { get; set; }

    public int PlantPhotoId { get; set; }

    public bool IsHealthy { get; set; }

    public decimal HealthProbability { get; set; }

    public string AiProvider { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<DiagnosisProblemDto> Problems { get; set; } = new();
}