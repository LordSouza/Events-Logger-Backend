using System.Net;
using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests.Entry;
using EventsLogger.Entities.Dtos.Requests.UserProject;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventsLogger.Api.Controllers;

[Route("api/Project/Entry")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProjectEntriesController : BaseController
{
    private readonly APIResponse _response;

    public ProjectEntriesController(IUnitOfWork unitOfWork,
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
    /// EndPoint <c>CreateEntryProject<\c> allow a manager or owner create a entry for another user wich they are part of
    /// </summary>
    /// <param name="createEntryProjectDTO"></param>
    /// <returns></returns>
    [HttpPost(Name = "CreateEntryProject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> CreateEntryProject([FromForm] CreateEntryProjectDTO createEntryProjectDTO)
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
                _response.Messages.Add("Your request is missing one or more parameter.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == createEntryProjectDTO.ProjectId);
            if (project == null)
            {
                _response.Messages.Add("A project with this ID does not exists.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            var user = await _unitOfWork.Users.GetAsync(u => u.Id == createEntryProjectDTO.UserId);
            if (user == null)
            {
                _response.Messages.Add("A user with this ID does not exists.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            UserProject UserPermission = await _unitOfWork.UsersProjects.GetAsync(u => u.ProjectId == project.Id && u.UserId == loggedUser.Id);
            if (UserPermission == null)
            {
                _response.Messages.Add("You are not part of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            UserProject userProject = await _unitOfWork.UsersProjects.GetAsync(u => u.ProjectId == project.Id && u.UserId == loggedUser.Id);
            // user with role worker in a project can't add others
            if (userProject == null || userProject.Role == "worker")
            {
                _response.Messages.Add("You are not part of the management of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            List<string> filesUrl = new();
            if (createEntryProjectDTO.Files != null)
            {
                foreach (var file in createEntryProjectDTO.Files)
                {
                    var url = await UploadFile(file);
                    filesUrl.Add(url);
                }
            }

            Entry newEntry = new()
            {
                UserId = user.Id,
                User = user,
                ProjectId = createEntryProjectDTO.ProjectId,
                Project = project,
                Description = createEntryProjectDTO.Description,
                FilesUrl = filesUrl,
            };

            await _unitOfWork.Entries.CreateAsync(newEntry);

            _response.Messages.Add("Created entry with success.");
            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<EntryDTO>(newEntry);

            return StatusCode(StatusCodes.Status201Created, _response);

        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return StatusCode(StatusCodes.Status500InternalServerError, _response);
    }


    /// <summary>
    /// Delete a entry the user has posted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}", Name = "DeleteEntry")]
    public async Task<ActionResult<APIResponse>> DeleteEntry(Guid id)
    {
        try
        {

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.Messages.Add("You are not logged in.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            var entryToDelete = await _unitOfWork.Entries.GetAsync(u => u.Id == id);

            if (entryToDelete == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages!.Add($"Entry with ID:{id} was not found");
                return NotFound(_response);
            }
            UserProject UserPermission = await _unitOfWork.UsersProjects.GetAsync(u => u.ProjectId == entryToDelete.ProjectId && loggedUser.Id == u.UserId);
            if (UserPermission == null)
            {
                _response.Messages.Add("You are not part of the project of this entry");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            if (UserPermission.Role == "worker")
            {
                _response.Messages.Add("You are not part of the management of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }


            await _unitOfWork.Entries.RemoveAsync(entryToDelete);

            _response.Messages.Add("Entry deleted with success.");
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return StatusCode(StatusCodes.Status500InternalServerError, _response);
    }

    /// <summary>
    /// updated a entry the user has posted
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entryUpdateDTO"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut("{id:guid}", Name = "UpdateEntry")]
    public async Task<ActionResult<APIResponse>> UpdateEntry(Guid id, [FromForm] UpdateEntryDTO entryUpdateDTO)
    {
        try
        {
            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.Messages.Add("You are not logged in.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            var entryToUpdate = await _unitOfWork.Entries.GetAsync(u => u.Id == id);

            if (entryToUpdate == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages!.Add($"Entry with ID:{id} was not found");
                return NotFound(_response);
            }

            UserProject UserPermission = await _unitOfWork.UsersProjects.GetAsync(u => u.ProjectId == entryToUpdate.ProjectId && loggedUser.Id == u.UserId);
            if (UserPermission == null)
            {
                _response.Messages.Add("You are not part of the project of this entry");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            if (UserPermission.Role == "worker")
            {
                _response.Messages.Add("You are not part of the management of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            Entry model = _mapper.Map<Entry>(entryUpdateDTO);

            await _unitOfWork.Entries.UpdateAsync(model);
            _response.Messages.Add("Entry update was successful.");
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return StatusCode(StatusCodes.Status204NoContent, _response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return StatusCode(StatusCodes.Status500InternalServerError, _response);
    }

}