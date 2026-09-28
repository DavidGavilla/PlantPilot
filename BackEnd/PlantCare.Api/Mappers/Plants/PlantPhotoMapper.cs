using PlantCare.Api.Models.Plants;

namespace PlantCare.Api.Mappers;

public static class PlantPhotoMapper
{
    // PlantPhoto -> PlantPhotoDto
    public static CreatePlantPhotoDto ToDto(PlantPhoto photo)
    {
        return new CreatePlantPhotoDto
        {
            PlantPhotoId = photo.PlantPhotoId,
            PlantId = photo.PlantId,
            ImageUrl = photo.ImageUrl,
            Source = photo.Source,
            CameraChannelId = photo.CameraChannelId,
            CapturedAt = photo.CapturedAt
        };
    }

    // PlantPhotoDto -> PlantPhoto
    public static PlantPhoto ToModel(CreatePlantPhotoDto dto)
    {
        return new PlantPhoto
        {
            PlantPhotoId = dto.PlantPhotoId,
            PlantId = dto.PlantId,
            ImageUrl = dto.ImageUrl,
            Source = dto.Source,
            CameraChannelId = dto.CameraChannelId,
            CapturedAt = dto.CapturedAt
        };
    }
}