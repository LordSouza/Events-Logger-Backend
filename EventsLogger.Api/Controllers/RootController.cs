using System.Net;
using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventsLogger.Api.Controllers;


[Route("")]
[ApiController]
public class RootController : BaseController
{
    private readonly APIResponse _response;

    public RootController(IUnitOfWork unitOfWork,
                              IMapper mapper,
                              UserManager<User> userManager,
                              IBlobManagement blobManagement,
                              IConfiguration configuration) : base(
                                  unitOfWork,
                                  mapper,
                                  userManager,
                                  blobManagement,
                                  configuration)
    {
        _response = new();
    }




    [HttpGet("{username}", Name = "GetUsersPublic")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> GetUser(string username)
    {
        try
        {

            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var user = await _unitOfWork.Users.GetAsync(u => u.UserName == username);

            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            ICollection<Entry> userEntries = await _unitOfWork.Entries.GetAllAsync(u => u.UserId == user.Id, includeProperties: "Project");

            var userToResponse = _mapper.Map<UserPublicDTO>(user);
            foreach (var entry in userEntries)
            {
                userToResponse.UserEntries.Add(_mapper.Map<EntryPublicDTO>(entry));
            }
            // UserList = await _unitOfWork.Users.GetAllAsync(u => u.UserName == username);
            _response.Result = userToResponse;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);



        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
    }
}