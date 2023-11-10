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
                              IConfiguration configuration) : base(
                                  unitOfWork,
                                  mapper,
                                  userManager,
                                  blobManagement,
                                  configuration)
    {
        _response = new();
    }



    [HttpGet("All", Name = "GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetUsers([FromQuery(Name = "UserName")] string? username)
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
            if (username != null)
            {
                UserList = await _unitOfWork.Users.GetAllAsync(u => u.UserName!.Contains(username));
            }
            else
            {
                UserList = await _unitOfWork.Users.GetAllAsync();
            }
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

            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
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


            await _unitOfWork.Users.RemoveAsync(loggedUser);

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


    // /// <summary>
    // /// Update self
    // /// </summary>
    // /// <param name="updateUserDTO"></param>
    // /// <returns></returns>
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [HttpPut(Name = "UpdateUser")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // public async Task<ActionResult<APIResponse>> UpdateUser([FromBody] UpdateUserDTO updateUserDTO)
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

    //         loggedUser.Name = updateUserDTO.FirstName + " " + updateUserDTO.LastName;
    //         loggedUser.Email = updateUserDTO.Email;
    //         loggedUser.PhotoPath = updateUserDTO.PhotoPath;

    //         await _userManager.UpdateAsync(loggedUser);

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



    /// <summary>
    /// This request wasn't tested after the changes
    /// </summary>
    /// <param name="patchDTO"></param>
    /// <returns></returns>
    // [HttpPatch("{Username}", Name = "UpdatePhoto")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<APIResponse>> UpdatePartialUser(string username, [FromForm] JsonPatchDocument<UpdateUserPhotoDTO> patchDTO)
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

    //         User userToChange = await _unitOfWork.Users.GetAsync(u => u.UserName == username);
    //         if (userToChange.Id != loggedUser.Id)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             return BadRequest(_response);
    //         }
    //         if (patchDTO.Operations[0].path != "/photopath")
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             return NotFound(_response);
    //         }

    //         // Url = UploadFile(patchDTO)
    //         // refactor this part
    //         UpdateUserDTO UserDTO = _mapper.Map<UpdateUserDTO>(loggedUser);

    //         // patchDTO.ApplyTo(UserDTO, ModelState);

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