using AutoMapper;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventsLogger.Api.Controllers;

//[Route("api/[controller]")]
[Route("api/Entry")]
[ApiController]
public class EntryAPIController : BaseController
{
    private readonly APIResponse _response;

    public EntryAPIController(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
        _response = new();
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetEntries()
    {
        try
        {
            IEnumerable<Entry> EntryList = await _unitOfWork.Entries.GetAllAsync();
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
            var Entry = await _unitOfWork.Entries.GetAsync(u => u.Id == id);
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
    public async Task<ActionResult<APIResponse>> CreateEntry([FromBody] CreateEntryDTO createEntryDTO)
    {
        try
        {
            if (createEntryDTO == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            Entry entry = _mapper.Map<Entry>(createEntryDTO);

            await _unitOfWork.Entries.CreateAsync(entry);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<EntryDTO>(entry);

            return CreatedAtRoute("GetEntry", new { id = entry.Id }, _response);
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

            var Entry = await _unitOfWork.Entries.GetAsync(u => u.Id == id);
            if (Entry == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            await _unitOfWork.Entries.RemoveAsync(Entry);

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

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut("{id:guid}", Name = "UpdateEntry")]
    public async Task<ActionResult<APIResponse>> UpdateEntry(Guid id, [FromBody] UpdateEntryDTO updateDTO)
    {
        try
        {

            if (updateDTO == null || id != updateDTO.Id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            Entry model = _mapper.Map<Entry>(updateDTO);

            await _unitOfWork.Entries.UpdateAsync(model);
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

    [HttpPatch("{id:guid}", Name = "UpdatePartialEntry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> UpdatePartialEntry(Guid id, JsonPatchDocument<UpdateEntryDTO> patchDTO)
    {
        try
        {

            var entry = await _unitOfWork.Entries.GetAsync(u => u.Id == id, false);

            if (entry is null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            UpdateEntryDTO EntryDTO = _mapper.Map<UpdateEntryDTO>(entry);

            patchDTO.ApplyTo(EntryDTO, ModelState);

            Entry model = _mapper.Map<Entry>(EntryDTO);
            model.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Entries.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
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

}