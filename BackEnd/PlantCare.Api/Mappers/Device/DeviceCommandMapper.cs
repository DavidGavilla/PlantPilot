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
            DeviceId = command.DeviceId,
            ChannelId = command.ChannelId,
            CommandType = command.CommandType,
            DurationSeconds = command.DurationSeconds,
            WaterAmountMl = command.WaterAmountMl,
            Status = command.Status,
            IdempotencyKey = command.IdempotencyKey,
            ExpiresAt = command.ExpiresAt,
            DateCreated = command.DateCreated,
            CompletedDate = command.CompletedDate
        };
    }

    public static DeviceCommand ToEntity(this CreateDeviceCommandDto dto)
    {
        return new DeviceCommand
        {
            DeviceId = dto.DeviceId,
            ChannelId = dto.ChannelId,
            CommandType = dto.CommandType,
            DurationSeconds = dto.DurationSeconds,
            WaterAmountMl = dto.WaterAmountMl,
            IdempotencyKey = dto.IdempotencyKey,
            ExpiresAt = dto.ExpiresAt
        };
    }
}