using PlantCare.Api.DTOs.Diagnoses;
using PlantCare.Api.Models.Diagnoses;

namespace PlantCare.Api.Mappers;

public static class DiagnosisMapper
{
    public static Diagnosis ToModel(this CreateDiagnosisDto dto)
    {
        return new Diagnosis
        {
            PlantPhotoId = dto.PlantPhotoId,
            HealthStatus = dto.HealthStatus,
            AnalysisStatus = dto.AnalysisStatus,
            HealthProbability = dto.HealthProbability,
            AiProvider = dto.AiProvider,
            AiModel = dto.AiModel,
            AiModelVersion = dto.AiModelVersion,
            Summary = dto.Summary,
            RawAiResponse = dto.RawAiResponse,

            Problems = dto.Problems
                .Select(p => p.ToModel())
                .ToList(),

            CreatedAt = DateTime.UtcNow
        };
    }

    public static DiagnosisDto ToDto(this Diagnosis diagnosis)
    {
        return new DiagnosisDto
        {
            DiagnosisId = diagnosis.DiagnosisId,
            PlantPhotoId = diagnosis.PlantPhotoId,
            HealthStatus = diagnosis.HealthStatus,
            AnalysisStatus = diagnosis.AnalysisStatus,
            HealthProbability = diagnosis.HealthProbability,
            AiProvider = diagnosis.AiProvider,
            AiModel = diagnosis.AiModel,
            AiModelVersion = diagnosis.AiModelVersion,
            Summary = diagnosis.Summary,
            CreatedAt = diagnosis.CreatedAt,

            Problems = diagnosis.Problems
                .Select(p => p.ToDto())
                .ToList()
        };
    }
}