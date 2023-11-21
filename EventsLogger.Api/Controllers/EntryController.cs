using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests.Entry;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventsLogger.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class EntryController : BaseController
{
    private readonly APIResponse _response;

    public EntryController(IUnitOfWork unitOfWork,
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
    /// Get all entries that is part of the project the user is also part of
    /// this request has filtering capabilities
    /// </summary>
    /// <param name="projectid"></param>
    /// <param name="userid"></param>
    /// <param name="datestart"></param>
    /// <param name="dateend"></param>
    /// <param name="hasfiles"></param>
    /// <param name="username"></param>
    /// <param name="projectname"></param>
    /// <param name="entrydescription"></param>
    /// <param name="usertimezone"></param>
    /// <returns></returns>
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetEntries(
        [FromQuery(Name = "ProjectId")] Guid? projectid,
        [FromQuery(Name = "UserId")] string? userid,
        [FromQuery(Name = "DatStart")] string? datestart,
        [FromQuery(Name = "DateEnd")] string? dateend,
        [FromQuery(Name = "HasFiles")] bool? hasfiles,
        [FromQuery(Name = "UserName")] string? username,
        [FromQuery(Name = "ProjectName")] string? projectname,
        [FromQuery(Name = "EntryDescription")] string? entrydescription,
        [FromHeader(Name = "Date")] string usertimezone
        )
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


            List<UserProject> projectList = await _unitOfWork.UsersProjects.GetAllAsync(u => u.UserId == loggedUser.Id);

            List<Entry> EntryList = new();

            foreach (var project in projectList)
            {
                EntryList.AddRange(await _unitOfWork.Entries.GetAllAsync(u => u.UserId == loggedUser.Id, includeProperties: "User,Project"));
            }
            IEnumerable<Entry> EntryListFiler = EntryList.AsEnumerable();

            if (entrydescription != null)
            {
                EntryListFiler = EntryListFiler.Where(u => u.Description.Contains(entrydescription));
            }

            if (username != null)
            {
                EntryListFiler = EntryListFiler.Where(u => u.User.Name.Contains(username));
            }

            if (projectname != null)
            {
                EntryListFiler = EntryListFiler.Where(u => u.Project.Name.Contains(projectname));
            }



            if (userid != null)
            {
                EntryListFiler = EntryListFiler.Where(u => u.UserId == userid);
            }


            if (projectid != null)
            {
                EntryListFiler = EntryListFiler.Where(u => u.ProjectId == projectid);
            }

            if (datestart != null)
            {
                var datestartfilter = DateTime.Parse(datestart);
                EntryListFiler = EntryListFiler.Where(u => u.CreatedDate > datestartfilter);
            }
            if (dateend != null)
            {
                var dateendfilter = DateTime.Parse(dateend);
                EntryListFiler = EntryListFiler.Where(u => u.CreatedDate < dateendfilter);
            }
            if (hasfiles != null)
            {
                EntryListFiler = EntryListFiler.Where(u => u.FilesUrl.Count > 0);
            }

            EntryListFiler = EntryListFiler.OrderByDescending(u => u.CreatedDate);

            _response.Result = _mapper.Map<IEnumerable<EntryDTO>>(EntryListFiler);
            _response.StatusCode = HttpStatusCode.OK;
            _response.Messages.Add("Success.");
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
    /// get entry with a id as parameter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}", Name = "GetEntry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetEntry(Guid id)
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



            var Entry = await _unitOfWork.Entries.GetAsync(u => u.Id == id, includeProperties: "User,Project");
            if (Entry == null)
            {
                _response.Messages.Add("There is no entry with this Id.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }


            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<EntryDTO>(Entry);
            _response.Messages.Add("Success.");
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
    /// Create a entry with the id of the user that made the request
    /// </summary>
    /// <param name="createEntryDTO"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> CreateEntry([FromForm] CreateEntryDTO createEntryDTO)
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

            if (!ModelState.IsValid)
            {
                _response.Messages.Add("Your request is missing one or more parameter.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            Project projectModel = await _unitOfWork.Projects.GetAsync(u => u.Id == createEntryDTO.ProjectId);

            if (projectModel == null)
            {
                _response.Messages.Add("There is no project with this Id.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            List<string> filesUrl = new();
            if (createEntryDTO.Files != null)
            {
                foreach (var file in createEntryDTO.Files)
                {
                    var url = await UploadFile(file);
                    filesUrl.Add(url);
                }
            }
            Entry newEntry = new()
            {
                UserId = loggedUser.Id,
                User = loggedUser,
                ProjectId = createEntryDTO.ProjectId,
                Project = projectModel,
                Description = createEntryDTO.Description,
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

            if (entryToDelete.UserId != loggedUser.Id)
            {
                _response.Messages.Add("You are not the creator of the entry");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Forbidden;
                return StatusCode(StatusCodes.Status403Forbidden, _response);
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

            if (!ModelState.IsValid)
            {
                _response.Messages.Add("Your request is missing one or more parameter.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var entryToChange = await _unitOfWork.Entries.GetAsync(u => u.Id == id);

            if (entryToChange == null)
            {
                _response.Messages.Add($"Entry with {id} was not found");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
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