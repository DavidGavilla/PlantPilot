namespace PlantCare.Api.Models.Diagnoses;

public class DiagnosisProblem
{
    public int ProblemId { get; set; }

    public int DiagnosisId { get; set; }

    public Diagnosis Diagnosis { get; set; } = null!;

    public string ProblemName { get; set; } = string.Empty;

    public decimal Probability { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Treatment { get; set; } = string.Empty;

    public string Prevention { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;
}