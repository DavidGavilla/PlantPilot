using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.DTOs.Devices;

public class CreateDeviceReadingDto
{
    public int ChannelId { get; set; }

    public ReadingType ReadingType { get; set; }

    public double Value { get; set; }

    public ReadingUnit Unit { get; set; }

    public DateTime MeasuredAt { get; set; }
}

public class DeviceReadingDto
{
    public int DeviceReadingId { get; set; }

    public int ChannelId { get; set; }

    public ReadingType ReadingType { get; set; }

    public double Value { get; set; }

    public ReadingUnit Unit { get; set; }

    public DateTime MeasuredAt { get; set; }

    public DateTime ReceivedAt { get; set; }
}