using PlantCare.Api.DTOs.Schedules;
using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.Mappers;

public static class ScheduleMapper
{
    public static Schedule ToModel(this CreateScheduleDto dto)
    {
        return new Schedule
        {
            PlantId = dto.PlantId,
            DiagnosisId = dto.DiagnosisId,
            DateAdded = DateTime.UtcNow,

            Tasks = dto.Tasks
                .Select(t => t.ToModel())
                .ToList()
        };
    }

    public static ScheduleDto ToDto(this Schedule schedule)
    {
        return new ScheduleDto
        {
            ScheduleId = schedule.ScheduleId,
            PlantId = schedule.PlantId,
            DiagnosisId = schedule.DiagnosisId,
            DateAdded = schedule.DateAdded,

            Tasks = schedule.Tasks
                .Select(t => t.ToDto())
                .ToList()
        };
    }
}