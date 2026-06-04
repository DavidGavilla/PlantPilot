namespace PlantCare.Api.DTOs.Schedules;

public class ScheduleTaskDto
{
    public int TaskId { get; set; }

    public DateTime Date { get; set; }

    public string Title { get; set; } = string.Empty;

    public string TaskType { get; set; } = string.Empty;

    public string TaskDescription { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}