namespace PlantCare.Api.Models.Schedules;

public class ScheduleTask
{
    public int TaskId { get; set; }

    public int ScheduleId { get; set; }

    public Schedule Schedule { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Title { get; set; } = string.Empty;

    public string TaskType { get; set; } = string.Empty;

    public string TaskDescription { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
}