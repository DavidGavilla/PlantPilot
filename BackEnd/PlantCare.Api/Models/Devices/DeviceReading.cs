namespace PlantCare.Api.Models.Devices;

public class DeviceReading
{
    public int DeviceReadingId { get; set; }

    public int PlantDeviceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ReadingType ReadingType { get; set; }

    public double Value { get; set; }

    public ReadingUnit Unit { get; set; }
}