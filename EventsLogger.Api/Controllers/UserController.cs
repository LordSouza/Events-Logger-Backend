using System.Net;
using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests.User;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace EventsLogger.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : BaseController
{
    private readonly APIResponse _response;

    public UserController(IUnitOfWork unitOfWork,
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



    /// <summary>
    /// Get all users,
    /// can filter by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("All", Name = "GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetUsers([FromQuery(Name = "UserName")] string? username)
    {
        try
        {

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.Messages.Add("You are not logged in");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            IEnumerable<User> UserList;
            if (username != null)
            {
                UserList = await _unitOfWork.Users.GetAllAsync(u => u.UserName!.Contains(username));
            }
            else
            {
                UserList = await _unitOfWork.Users.GetAllAsync();
            }
            _response.Result = _mapper.Map<List<UserDTO>>(UserList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);


        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.Unauthorized;
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return StatusCode(StatusCodes.Status500InternalServerError, _response);
    }




    /// <summary>
    /// Confirm password for self deletion
    /// </summary>
    /// <param name="userPasswordRequestDTO"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete(Name = "DeleteUser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> DeleteUser([FromBody] UserPasswordRequestDTO userPasswordRequestDTO)
    {
        try
        {

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.Messages.Add("You are not logged in");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            if (!ModelState.IsValid)
            {
                _response.Messages.Add("Your request needs to have your password.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            var isPassword = await _userManager.CheckPasswordAsync(loggedUser, userPasswordRequestDTO.Password);
            if (!isPassword)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages.Add("Wrong Password");
                return NotFound(_response);
            }


            await _unitOfWork.Users.RemoveAsync(loggedUser);

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.Unauthorized;
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return StatusCode(StatusCodes.Status500InternalServerError, _response);
    }


    /// <summary>
    /// Update self
    /// </summary>
    /// <param name="updateUserDTO"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut(Name = "UpdateUser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> UpdateUser([FromBody] UpdateUserDTO updateUserDTO)
    {
        try
        {
            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.Messages.Add("You are not logged in");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            if (!ModelState.IsValid)
            {
                _response.Messages.Add("Your request needs to have a ProjectId, Role, FirstName, LastName, Email, and can have a Photo, and UserName");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            loggedUser.Name = updateUserDTO.FirstName + " " + updateUserDTO.LastName ?? loggedUser.Name;
            loggedUser.Email = updateUserDTO.Email ?? loggedUser.Email;
            loggedUser.PhotoPath = updateUserDTO.PhotoPath ?? loggedUser.PhotoPath;

            if (updateUserDTO.Password != null && updateUserDTO.NewPassword != null)
            {
                await _userManager.ChangePasswordAsync(loggedUser, updateUserDTO.Password, updateUserDTO.NewPassword);
            }

            await _userManager.UpdateAsync(loggedUser);

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.Unauthorized;
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return StatusCode(StatusCodes.Status500InternalServerError, _response);
    }






}