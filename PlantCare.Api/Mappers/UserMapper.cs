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

            // Do NOT map PasswordHash here
            // Hash the password in the service
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
        user.Name = dto.Name;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
    }
}