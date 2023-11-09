using AutoMapper;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests;
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
public class EntryController : BaseController
{
    private readonly APIResponse _response;

    public EntryController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager) : base(unitOfWork, mapper, userManager)
    {
        _response = new();
    }


    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetEntries([FromQuery] Guid? projectid, [FromQuery] string? userid)
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
            IEnumerable<Entry> EntryList;
            if (userid != null && projectid == null)
            {
                EntryList = await _unitOfWork.Entries.GetAllAsync(u => u.UserId == userid, includeProperties: "User,Project");
                _response.Result = _mapper.Map<List<EntryDTO>>(EntryList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }


            if (userid == null && projectid != null)
            {
                EntryList = await _unitOfWork.Entries.GetAllAsync(u => u.ProjectId == projectid, includeProperties: "User,Project");
                _response.Result = _mapper.Map<List<EntryDTO>>(EntryList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }

            if (userid != null && projectid != null)
            {
                EntryList = await _unitOfWork.Entries.GetAllAsync(u => u.UserId == userid && u.ProjectId == projectid, includeProperties: "User,Project");
                _response.Result = _mapper.Map<List<EntryDTO>>(EntryList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            EntryList = await _unitOfWork.Entries.GetAllAsync(includeProperties: "User,Project");
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


    [HttpGet(Name = "GetEntry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetEntry(GetByIdDTO entryIdDTO)
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
            var Entry = await _unitOfWork.Entries.GetAsync(u => u.Id == entryIdDTO.Id, includeProperties: "User,Project");
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> CreateEntry([FromBody] CreateEntryDTO createEntryDTO)
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

            Entry newEntry = new()
            {
                UserId = profile.Id,
                User = profile,
                ProjectId = createEntryDTO.ProjectId,
                Project = projectModel,
                Description = createEntryDTO.Description,
                FilesUrl = createEntryDTO.FilesUrl,
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
    [HttpDelete(Name = "DeleteEntry")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> DeleteEntry([FromBody] GetByIdDTO entryIdDTO)
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




            var entryToDelete = await _unitOfWork.Entries.GetAsync(u => u.Id == entryIdDTO.Id);

            if (entryToDelete.UserId != profile.Id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            if (entryToDelete == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
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