using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests.User;

public class UpdateUserPhotoDTO
{
    public IFormFile? Photo { get; set; }

}