using System.Data.Common;
using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventsLogger.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{

    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    public UserManager<User> _userManager;
    private readonly IBlobManagement _blobManagement;
    private readonly IQueuesManagement _queuesManagement;
    private readonly IConfiguration _configuration;

    public BaseController(
        IUnitOfWork unitOfWork,
         IMapper mapper,
         UserManager<User> userManager,
         IBlobManagement blobManagement,
         IQueuesManagement queuesManagement,
         IConfiguration configuration)
    {
        _configuration = configuration;
        _queuesManagement = queuesManagement;
        _blobManagement = blobManagement;
        _userManager = userManager;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [NonAction]
    private async Task<string> UploadFile(IFormFile img, int width, int height)
    {
        if (img is not { Length: > 0 }) return "";

        var connection = _configuration["StorageConfig:BlobConnection"];

        MemoryStream? stream = null;

        byte[]? fileBytes = null;
        await using (stream = new MemoryStream())
        {
            await img.CopyToAsync(stream);
            fileBytes = stream.ToArray();
        }

        if (fileBytes == null) return "";

        var name = Path.GetRandomFileName() + "_" + DateTime.UtcNow.ToString("dd/MM/yyyy").Replace("/", "_");

        var url = await _blobManagement.UploadFile("files", name, fileBytes, connection);

        return url;

    }

}
