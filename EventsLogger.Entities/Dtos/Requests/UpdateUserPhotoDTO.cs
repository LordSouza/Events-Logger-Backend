using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests;

public class UpdateUserPhotoDTO
{
    public IFormFile? Photo { get; set; }

}