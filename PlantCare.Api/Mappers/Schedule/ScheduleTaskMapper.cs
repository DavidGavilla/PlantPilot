using PlantCare.Api.DTOs.Schedules;
using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.Mappers;

public static class ScheduleTaskMapper
{
    public static ScheduleTask ToModel(this CreateScheduleTaskDto dto)
    {
        return new ScheduleTask
        {
            Date = dto.Date,
            Title = dto.Title,
            TaskType = dto.TaskType,
            TaskDescription = dto.TaskDescription,
            IsCompleted = false,
            CompletedAt = null
        };
    }

    public static ScheduleTaskDto ToDto(this ScheduleTask task)
    {
        return new ScheduleTaskDto
        {
            TaskId = task.TaskId,
            Date = task.Date,
            Title = task.Title,
            TaskType = task.TaskType,
            TaskDescription = task.TaskDescription,
            IsCompleted = task.IsCompleted
        };
    }

    public static void UpdateModel(this ScheduleTask task, UpdateScheduleTaskDto dto)
    {
        task.IsCompleted = dto.IsCompleted;

        task.CompletedAt = dto.IsCompleted
            ? DateTime.UtcNow
            : null;
    }
}