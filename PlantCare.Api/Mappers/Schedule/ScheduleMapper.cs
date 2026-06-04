using PlantCare.Api.DTOs.Schedules;
using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.Mappers.Schedule;

public static class ScheduleMapper
{
    public static Schedules ToModel(this CreateScheduleDto dto)
    {
        return new Schedule
        {
            PlantId = dto.PlantId,
            TaskType = dto.TaskType,
            Description = dto.Description,
            ScheduledAt = dto.ScheduledAt
        };
    }

}