using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.DTOs.Schedules;

public class ScheduleTaskDto
{
    public int TaskId { get; set; }

    public DateTime Date { get; set; }

    public string Title { get; set; } = string.Empty;

    public string TaskType { get; set; } = string.Empty;

    public string TaskDescription { get; set; } = string.Empty;

    public ScheduleTaskStatus Status { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }
}