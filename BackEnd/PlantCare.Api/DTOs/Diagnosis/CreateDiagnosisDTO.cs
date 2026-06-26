namespace PlantCare.Api.DTOs.Diagnoses;

public class CreateDiagnosisDto
{
    public int PlantPhotoId { get; set; }

    public bool IsHealthy { get; set; }

    public decimal HealthProbability { get; set; }

    public string AiProvider { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string RawAiResponse { get; set; } = string.Empty;

    public List<CreateDiagnosisProblemDto> Problems { get; set; } = new();
}