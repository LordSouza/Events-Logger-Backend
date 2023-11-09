using System.Net;
using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests;
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
                              IQueuesManagement queuesManagement,
                              IConfiguration configuration) : base(
                                  unitOfWork,
                                  mapper,
                                  userManager,
                                  blobManagement,
                                  queuesManagement,
                                  configuration)
    {
        _response = new();
    }

    [HttpGet("All", Name = "GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetUsers([FromQuery(Name = "UserName")] string? username, [FromQuery(Name = "Project")] string? project)
    {
        try
        {

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            var profile = await _unitOfWork.Users.GetAsync(u => u.Id == loggedUser.Id);
            if (profile == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            IEnumerable<User> UserList;
            // IEnumerable<UserProject> UserProjectList;
            UserList = await _unitOfWork.Users.GetAllAsync();
            // UserList = await _unitOfWork.Users.GetAllAsync(u => u.UserName == username);
            _response.Result = _mapper.Map<List<UserDTO>>(UserList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);

            // if (project != null)
            // {

            //     // UserProjectList = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == project);
            //     UserList = await _unitOfWork.Users.GetAllAsync(u => u.Name == name);
            //     _response.Result = _mapper.Map<List<UserDTO>>(UserList);
            //     _response.StatusCode = HttpStatusCode.OK;
            //     return Ok(_response);
            // }


        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
    }


    /// <summary>
    /// Get self data if no query is made
    /// 
    /// if name = all 
    /// {     
    ///     query all users
    ///}
    /// 
    /// if name = {username} 
    /// {
    ///     query user with username      
    /// }
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetUserByUsername")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetUser([FromQuery(Name = "UserName")] string? username)
    {
        try
        {

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            var profile = await _unitOfWork.Users.GetAsync(u => u.Id == loggedUser.Id);
            if (profile == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            // IEnumerable<UserProject> UserProjectList;
            if (username == null)
            {
                User self = await _unitOfWork.Users.GetAsync(u => u.UserName == profile.UserName);

                _response.Result = _mapper.Map<UserDTO>(self);
                _response.StatusCode = HttpStatusCode.OK;
                UserDTO selfDTO = _mapper.Map<UserDTO>(profile);
                return Ok(_response);

            }

            User userByUsername = await _unitOfWork.Users.GetAsync(u => u.UserName == username);

            _response.Result = _mapper.Map<UserDTO>(userByUsername);
            _response.StatusCode = HttpStatusCode.OK;
            UserDTO profileDTO = _mapper.Map<UserDTO>(profile);
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
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
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }



            var profile = await _unitOfWork.Users.GetAsync(u => u.Id == loggedUser.Id);
            if (profile == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }


            var isPassword = await _userManager.CheckPasswordAsync(loggedUser, userPasswordRequestDTO.Password);
            if (!isPassword)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages.Add("Wrong Password");
                return NotFound(_response);
            }


            await _unitOfWork.Users.RemoveAsync(profile);

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
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
            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            var profile = await _unitOfWork.Users.GetAsync(u => u.Id == loggedUser.Id);
            if (profile == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }



            profile.Name = updateUserDTO.FirstName + " " + updateUserDTO.LastName;
            profile.Email = updateUserDTO.Email;
            profile.PhotoPath = updateUserDTO.PhotoPath;

            await _userManager.UpdateAsync(profile);

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
    }

    // TODO change password post request



    /// <summary>
    /// This request wasn't tested after the changes
    /// </summary>
    /// <param name="patchDTO"></param>
    /// <returns></returns>
    // [HttpPatch(Name = "UpdatePartialUser")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<APIResponse>> UpdatePartialUser(JsonPatchDocument<UpdateUserDTO> patchDTO)
    // {
    //     try
    //     {
    //         if (!ModelState.IsValid)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             return BadRequest(_response);
    //         }

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

    //         // refactor this part
    //         UpdateUserDTO UserDTO = _mapper.Map<UpdateUserDTO>(User);

    //         patchDTO.ApplyTo(UserDTO, ModelState);

    //         User model = _mapper.Map<User>(UserDTO);

    //         await _unitOfWork.Users.UpdateAsync(model);

    //         if (!ModelState.IsValid)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             return BadRequest(_response);
    //         }
    //         _response.StatusCode = HttpStatusCode.NoContent;
    //         _response.IsSuccess = true;
    //         return Ok(_response);
    //     }
    //     catch (Exception ex)
    //     {
    //         _response.IsSuccess = false;
    //         _response.Messages = new List<string> { ex.ToString() };
    //     }
    //     return _response;
    // }


}