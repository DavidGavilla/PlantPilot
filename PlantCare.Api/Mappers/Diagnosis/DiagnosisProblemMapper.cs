using PlantCare.Api.DTOs.Diagnoses;
using PlantCare.Api.Models.Diagnoses;

namespace PlantCare.Api.Mappers;

public static class DiagnosisProblemMapper
{
    public static DiagnosisProblem ToModel(this CreateDiagnosisProblemDto dto)
    {
        return new DiagnosisProblem
        {
            ProblemName = dto.ProblemName,
            Probability = dto.Probability,
            Description = dto.Description,
            Treatment = dto.Treatment,
            Prevention = dto.Prevention,
            Severity = dto.Severity
        };
    }

    public static DiagnosisProblemDto ToDto(this DiagnosisProblem problem)
    {
        return new DiagnosisProblemDto
        {
            ProblemId = problem.ProblemId,
            ProblemName = problem.ProblemName,
            Probability = problem.Probability,
            Description = problem.Description,
            Treatment = problem.Treatment,
            Prevention = problem.Prevention,
            Severity = problem.Severity
        };
    }
}