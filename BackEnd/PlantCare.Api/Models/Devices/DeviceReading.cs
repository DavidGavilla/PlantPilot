namespace PlantCare.Api.Models.Devices;

public class DeviceReading
{
    public int DeviceReadingId { get; set; }

    public int ChannelId { get; set; }

    public DeviceChannel Channel { get; set; } = null!;

    public ReadingType ReadingType { get; set; }

    public double Value { get; set; }

    public ReadingUnit Unit { get; set; }

    public DateTime MeasuredAt { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}