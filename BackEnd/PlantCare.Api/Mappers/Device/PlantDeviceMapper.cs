using PlantCare.Api.DTOs.Devices;
using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.Mappers.Devices;

public static class PlantDeviceMapper
{
    public static PlantDeviceDto ToDto(this PlantDevice plantDevice)
    {
        return new PlantDeviceDto
        {
            PlantDeviceId = plantDevice.PlantDeviceId,
            DeviceId = plantDevice.DeviceId,
            PlantId = plantDevice.PlantId
        };
    }

    public static PlantDevice ToEntity(this CreatePlantDeviceDto dto)
    {
        return new PlantDevice
        {
            DeviceId = dto.DeviceId,
            PlantId = dto.PlantId
        };
    }
}