using System.Net;
using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventsLogger.Api.Controllers;

[Route("[controller]")]
public class ReportController : BaseController
{
    private readonly APIResponse _response;

    public ReportController(IUnitOfWork unitOfWork,
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

    // [HttpGet()]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // public async Task<ActionResult<APIResponse>> GetReport(
    //     [FromQuery(Name = "UserName")] string? username,
    //     [FromQuery(Name = "Project")] string? project,
    //     [FromQuery(Name = "DateStart")] string? datestart,
    //     [FromQuery(Name = "DateEnd")] string? dateend)
    // {
    //     try
    //     {

    //         var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
    //         if (loggedUser == null)
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             return NotFound(_response);
    //         }

    //         var profile = await _unitOfWork.Users.GetAsync(u => u.Id == loggedUser.Id);
    //         if (profile == null)
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             return NotFound(_response);
    //         }
    //         IEnumerable<User> UserList;
    //         // IEnumerable<UserProject> UserProjectList;
    //         if (username != null)
    //         {
    //             UserList = await _unitOfWork.Users.GetAllAsync(u => u.UserName!.Contains(username));
    //         }
    //         else
    //         {
    //             UserList = await _unitOfWork.Users.GetAllAsync();
    //         }
    //         // UserList = await _unitOfWork.Users.GetAllAsync(u => u.UserName == username);
    //         // _response.Result = _mapper.Map<List<UserDTO>>(UserList);
    //         _response.StatusCode = HttpStatusCode.OK;
    //         return Ok(_response);

    //         // if (project != null)
    //         // {

    //         //     // UserProjectList = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == project);
    //         //     UserList = await _unitOfWork.Users.GetAllAsync(u => u.Name == name);
    //         //     _response.Result = _mapper.Map<List<UserDTO>>(UserList);
    //         //     _response.StatusCode = HttpStatusCode.OK;
    //         //     return Ok(_response);
    //         // }


    //     }
    //     catch (Exception ex)
    //     {
    //         _response.IsSuccess = false;
    //         _response.Messages = new List<string> { ex.ToString() };
    //     }
    //     return _response;
    // }


}