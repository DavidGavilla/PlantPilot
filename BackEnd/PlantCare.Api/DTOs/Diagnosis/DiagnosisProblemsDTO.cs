namespace PlantCare.Api.DTOs.Diagnoses;

public class DiagnosisProblemDto
{
    public int ProblemId { get; set; }

    public string ProblemName { get; set; } = string.Empty;

    public decimal Probability { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Treatment { get; set; } = string.Empty;

    public string Prevention { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;
}