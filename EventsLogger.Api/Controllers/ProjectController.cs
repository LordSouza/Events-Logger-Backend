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

//[Route("api/[controller]")]
[Route("api/Project")]
[ApiController]
public class ProjectAPIController : BaseController
{
    private readonly APIResponse _response;

    public ProjectAPIController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager) : base(unitOfWork, mapper, userManager)
    {
        _response = new();
    }

    // [HttpGet]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // public async Task<ActionResult<APIResponse>> GetProjects()
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

    //         IEnumerable<UserProject> userProjects = await _unitOfWork.UsersProjects.GetAllAsync(u => u.UserId == profile.Id);
    //         // IEnumerable<Project> ProjectList = await _unitOfWork.Projects.GetAllAsync(includeProperties: "Address");

    //         // _response.Result = _mapper.Map<List<ProjectDTO>>(ProjectList);
    //         _response.StatusCode = HttpStatusCode.OK;
    //         return Ok(_response);
    //     }
    //     catch (Exception ex)
    //     {
    //         _response.IsSuccess = false;
    //         _response.Messages = new List<string> { ex.ToString() };
    //     }
    //     return _response;
    // }

    [HttpPost("AddUser/{id:guid}", Name = "AddUser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> AddUser(Guid id, [FromBody] CreateUserProjectDTO createUserProjectDTO)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id, includeProperties: "Address");
            if (project == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            UserProject relationship = _mapper.Map<UserProject>(createUserProjectDTO);
            await _unitOfWork.UsersProjects.CreateAsync(relationship);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<UserProjectDTO>(relationship);

            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
    }




    [HttpGet(Name = "GetProject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> GetProject([FromBody] string id)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == new Guid(id));

            if (project == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            var address = await _unitOfWork.Addresses.GetAsync(u => u.Id == project.AddressId);


            var projectDTO = _mapper.Map<Project>(project);
            projectDTO.Address = address;

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = projectDTO;
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
    public async Task<ActionResult<APIResponse>> CreateProject([FromBody] CreateProjectDTO createProjectDTO)
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


            // TODO refactor the DTO mapping


            Address newAddress = new()
            {
                State = createProjectDTO.Address.State,
                Street = createProjectDTO.Address.Street,
                City = createProjectDTO.Address.City,
                ZipCode = createProjectDTO.Address.ZipCode,
                Country = createProjectDTO.Address.Country,
            };

            Project newProject = new()
            {
                Name = createProjectDTO.Name,
                Address = newAddress,
                AddressId = newAddress.Id
            };

            await _unitOfWork.Addresses.CreateAsync(newAddress);
            await _unitOfWork.Projects.CreateAsync(newProject);

            // add the user to the project
            UserProject newUserProject = new()
            {
                CreatedDate = DateTime.UtcNow,
                UserId = profile.Id,
                ProjectId = newProject.Id,
                Role = "owner"
            };

            await _unitOfWork.UsersProjects.CreateAsync(newUserProject);


            var projectDTO = _mapper.Map<Project>(newProject);



            _response.StatusCode = HttpStatusCode.Created;
            _response.Messages.Add("The Project was created with success");
            _response.Result = projectDTO;

            return CreatedAtRoute("GetProject", new { id = newProject.Id }, _response);
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
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [HttpDelete("{id:guid}", Name = "DeleteProject")]
    // public async Task<ActionResult<APIResponse>> DeleteProject(Guid id)
    // {
    //     try
    //     {

    //         var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id);
    //         if (project == null)
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             _response.IsSuccess = false;
    //             _response.Messages!.Add("The project with ID was not found");
    //             return NotFound(_response);
    //         }
    //         // remove address
    //         project.Address = await _unitOfWork.Addresses.GetAsync(u => u.Id == project.AddressId, false);
    //         //await _unitOfWork.Addresses.RemoveAsync(project.Address);
    //         await _unitOfWork.Projects.RemoveAsync(project);

    //         _response.StatusCode = HttpStatusCode.NoContent;
    //         _response.Messages!.Add("The Project was deleted");
    //         _response.IsSuccess = true;
    //         return Ok(_response);
    //     }
    //     catch (Exception ex)
    //     {
    //         _response.StatusCode = HttpStatusCode.InternalServerError;
    //         _response.IsSuccess = false;
    //         _response.Messages = new List<string> { ex.ToString() };
    //     }
    //     return _response;
    // }


    // [HttpPatch("{id:guid}", Name = "UpdatePartialProject")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<APIResponse>> UpdatePartialProject(Guid id, JsonPatchDocument<UpdateProjectDTO> patchDTO)
    // {
    //     try
    //     {

    //         if (id == Guid.Empty)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             _response.Messages!.Add("The Id is invalid");
    //             _response.IsSuccess = false;
    //             return BadRequest(_response);
    //         }
    //         var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id, false);

    //         if (project == null)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             _response.Messages!.Add("The project doesn't exists");
    //             _response.IsSuccess = false;
    //             return BadRequest(_response);
    //         }

    //         UpdateProjectDTO projectDTO = _mapper.Map<UpdateProjectDTO>(project);

    //         patchDTO.ApplyTo(projectDTO, ModelState);

    //         Project model = _mapper.Map<Project>(projectDTO);



    //         if (patchDTO.Operations[0].path == "/address")
    //         {
    //             Address address = new()
    //             {
    //                 Id = model.AddressId,
    //                 State = model.Address.State,
    //                 Street = model.Address.Street,
    //                 City = model.Address.City,
    //                 ZipCode = model.Address.ZipCode,
    //                 Country = model.Address.Country,
    //             };
    //             await _unitOfWork.Addresses.UpdateAsync(address);

    //         }
    //         else
    //         {
    //             model.Address = await _unitOfWork.Addresses.GetAsync(u => u.Id == model.AddressId, false);
    //             await _unitOfWork.Projects.UpdateAsync(model);
    //         }


    //         if (!ModelState.IsValid)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             _response.IsSuccess = false;
    //             return BadRequest(_response);
    //         }
    //         _response.StatusCode = HttpStatusCode.NoContent;
    //         _response.Messages!.Add("The patch was applied with success");
    //         _response.IsSuccess = true;
    //         return Ok(_response);
    //     }
    //     catch (Exception ex)
    //     {
    //         _response.StatusCode = HttpStatusCode.InternalServerError;
    //         _response.IsSuccess = false;
    //         _response.Messages = new List<string> { ex.ToString() };
    //     }
    //     return _response;
    // }

}