
using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Models.Plants;

namespace PlantCare.Api.Mappers;

public static class PlantPhotoMapper
{
    // PlantPhoto -> PlantPhotoDto
    public static PlantPhotoDto ToDto(PlantPhoto photo)
    {
        return new PlantPhotoDto
        {
            PlantPhotoId = photo.PlantPhotoId,
            PlantId = photo.PlantId,
            PhotoUrl = photo.ImageUrl,
            DateAdded = photo.Date
        };
    }

    // PlantPhotoDto -> PlantPhoto
    public static PlantPhoto ToModel(PlantPhotoDto dto)
    {
        return new PlantPhoto
        {
            PlantId = dto.PlantId,
            ImageUrl = dto.PhotoUrl,
            Date = DateTime.UtcNow
        };
    }
}