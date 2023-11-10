using System.Net;
using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventsLogger.Api.Controllers;


public class BaseController : ControllerBase
{

    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    public UserManager<User> _userManager;
    protected readonly IBlobManagement _blobManagement;
    protected readonly IConfiguration _configuration;

    public BaseController(
        IUnitOfWork unitOfWork,
         IMapper mapper,
         UserManager<User> userManager,
         IBlobManagement blobManagement,
         IConfiguration configuration)
    {
        _configuration = configuration;
        _blobManagement = blobManagement;
        _userManager = userManager;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [NonAction]
    protected async Task<string> UploadFile(IFormFile fileToUpload)
    {
        if (fileToUpload is not { Length: > 0 }) return "";

        var connection = _configuration["StorageConfig:BlobConnection"];

        MemoryStream? stream = null;

        byte[]? fileBytes = null;
        await using (stream = new MemoryStream())
        {
            await fileToUpload.CopyToAsync(stream);
            fileBytes = stream.ToArray();
        }

        if (fileBytes == null) return "";

        var fileExtension = Path.GetExtension(fileToUpload.FileName);
        var name = Path.GetRandomFileName() + "_" + DateTime.UtcNow.ToString("dd/MM/yyyy").Replace("/", "_") + fileExtension;

        var url = await _blobManagement.UploadFile("files", name, fileBytes, connection);

        return url;

    }



}
