using PlantCare.Api.DTOs.Devices;
using PlantCare.Api.Models.Devices;

namespace PlantCare.Api.Mappers.Devices;

public static class DeviceCommandMapper
{
    public static DeviceCommandDto ToDto(this DeviceCommand command)
    {
        return new DeviceCommandDto
        {
            DeviceCommandId = command.DeviceCommandId,
            PlantDeviceId = command.PlantDeviceId,
            CommandType = command.CommandType,
            DurationSeconds = command.DurationSeconds,
            WaterAmountMl = command.WaterAmountMl,
            Status = command.Status,
            DateCreated = command.DateCreated,
            CompletedDate = command.CompletedDate
        };
    }

    public static DeviceCommand ToEntity(this CreateDeviceCommandDto dto)
    {
        return new DeviceCommand
        {
            PlantDeviceId = dto.PlantDeviceId,
            CommandType = dto.CommandType,
            DurationSeconds = dto.DurationSeconds,
            WaterAmountMl = dto.WaterAmountMl
        };
    }
}