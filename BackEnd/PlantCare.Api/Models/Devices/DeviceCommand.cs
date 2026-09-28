namespace PlantCare.Api.Models.Devices;

public class DeviceCommand
{
    public int DeviceCommandId { get; set; }

    public int DeviceId { get; set; }

    public Device Device { get; set; } = null!;

    public int? ChannelId { get; set; }

    public DeviceChannel? Channel { get; set; }

    public DeviceCommandType CommandType { get; set; }

    public int? DurationSeconds { get; set; }

    public int? WaterAmountMl { get; set; }

    public CommandStatus Status { get; set; } = CommandStatus.Pending;

    // Client-supplied, unique per logical command intent: lets a retried "send command"
    // request be recognized instead of creating a duplicate command. Nullable: a command
    // migrated from before idempotency keys existed has no known key.
    public string? IdempotencyKey { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedDate { get; set; }
}