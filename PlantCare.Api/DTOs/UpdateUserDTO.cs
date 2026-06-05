namespace PlantCare.Api.DTOs.Users;

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}