using PlantCare.Api.DTOs.Users;
using PlantCare.Api.Models;

namespace PlantCare.Api.Mappers;

public static class UserMapper
{
    public static User ToModel(this RegisterUserDto dto)
    {
        return new User
        {
            Name = dto.Name,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber


        };
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    public static void UpdateModel(this User user, UpdateUserDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = dto.LastName;

        if (dto.PhoneNumber is not null)
            user.PhoneNumber = dto.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(dto.Gmail))
            user.Email = dto.Gmail;
    }
}
    
