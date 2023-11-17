using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests.Entry;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
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


    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<APIResponse>> GetEntries(
        [FromQuery(Name = "ProjectId")] Guid? projectid,
        [FromQuery(Name = "UserId")] string? userid,
        [FromQuery(Name = "DatStart")] string? datestart,
        [FromQuery(Name = "DateEnd")] string? dateend,
        [FromQuery(Name = "HasFiles")] bool? hasfiles)
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
                EntryList.AddRange(await _unitOfWork.Entries.GetAllAsync(u => u.UserId == userid, includeProperties: "User,Project"));
            }

            IEnumerable<Entry> EntryListFiler = EntryList.AsEnumerable();

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
                EntryListFiler = EntryListFiler.Where(u => u.FilesUrl.Count == 0);
            }

            EntryListFiler = EntryListFiler.OrderByDescending(u => u.CreatedDate);

            _response.Result = _mapper.Map<List<EntryDTO>>(EntryList);
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


    [HttpGet("{id:guid}", Name = "GetEntry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> GetEntry(Guid id)
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

            var Entry = await _unitOfWork.Entries.GetAsync(u => u.Id == id, includeProperties: "User,Project");
            if (Entry == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<EntryDTO>(Entry);
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> CreateEntry([FromForm] CreateEntryDTO createEntryDTO)
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
            Project projectModel = await _unitOfWork.Projects.GetAsync(u => u.Id == createEntryDTO.ProjectId);

            if (projectModel == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            List<string> filesUrl = new();

            foreach (var file in createEntryDTO.Files)
            {
                var url = await UploadFile(file);
                filesUrl.Add(url);
            }
            Entry newEntry = new()
            {
                UserId = profile.Id,
                User = profile,
                ProjectId = createEntryDTO.ProjectId,
                Project = projectModel,
                Description = createEntryDTO.Description,
                FilesUrl = filesUrl,
            };

            await _unitOfWork.Entries.CreateAsync(newEntry);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<EntryDTO>(newEntry);

            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
    }


    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}", Name = "DeleteEntry")]
    public async Task<ActionResult<APIResponse>> DeleteEntry(Guid id)
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


            var entryToDelete = await _unitOfWork.Entries.GetAsync(u => u.Id == id);

            if (entryToDelete == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            if (entryToDelete.UserId != loggedUser.Id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            await _unitOfWork.Entries.RemoveAsync(entryToDelete);

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

    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [HttpPut("{id:guid}", Name = "UpdateEntry")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // public async Task<ActionResult<APIResponse>> UpdateEntry(Guid id, [FromBody] UpdateEntryDTO updateDTO)
    // {
    //     try
    //     {

    //         if (updateDTO == null || id != updateDTO.Id)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             return BadRequest(_response);
    //         }
    //         Entry model = _mapper.Map<Entry>(updateDTO);

    //         await _unitOfWork.Entries.UpdateAsync(model);
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

    // [HttpPatch(Name = "UpdatePartialEntry")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // public async Task<ActionResult<APIResponse>> UpdatePartialEntry(Guid id, JsonPatchDocument<UpdateEntryDTO> patchDTO)
    // {
    //     try
    //     {

    //         var entry = await _unitOfWork.Entries.GetAsync(u => u.Id == id, false);

    //         if (entry is null)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             return BadRequest(_response);
    //         }
    //         UpdateEntryDTO EntryDTO = _mapper.Map<UpdateEntryDTO>(entry);

    //         patchDTO.ApplyTo(EntryDTO, ModelState);

    //         Entry model = _mapper.Map<Entry>(EntryDTO);
    //         model.UpdatedDate = DateTime.UtcNow;

    //         await _unitOfWork.Entries.UpdateAsync(model);

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