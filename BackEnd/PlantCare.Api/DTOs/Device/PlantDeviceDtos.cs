namespace PlantCare.Api.DTOs.Devices;

public class CreatePlantDeviceDto
{
    public int DeviceId { get; set; }

    public int PlantId { get; set; }
}

public class PlantDeviceDto
{
    public int PlantDeviceId { get; set; }

    public int DeviceId { get; set; }

    public int PlantId { get; set; }
}