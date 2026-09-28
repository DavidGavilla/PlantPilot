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
            Status = ScheduleTaskStatus.Pending,
            CompletedAt = null,
            CancelledAt = null
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
            Status = task.Status,
            CompletedAt = task.CompletedAt,
            CancelledAt = task.CancelledAt
        };
    }

    public static void UpdateModel(this ScheduleTask task, UpdateScheduleTaskDto dto)
    {
        task.Status = dto.Status;

        task.CompletedAt = dto.Status == ScheduleTaskStatus.Completed
            ? DateTime.UtcNow
            : task.CompletedAt;

        task.CancelledAt = dto.Status == ScheduleTaskStatus.Cancelled
            ? DateTime.UtcNow
            : task.CancelledAt;
    }
}