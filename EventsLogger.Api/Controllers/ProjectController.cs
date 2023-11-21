using AutoMapper;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests.Project;
using EventsLogger.Entities.Dtos.Requests.UserProject;
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
public class ProjectController : BaseController
{
    private readonly APIResponse _response;

    public ProjectController(IUnitOfWork unitOfWork,
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
    /// EndPoint <c>GetProjects<\c> Get all projects that the user is part of
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetProjects()
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


            IEnumerable<UserProject> userProjects = await _unitOfWork.UsersProjects.GetAllAsync(u => u.UserId == loggedUser.Id, includeProperties: "Project");
            _response.Result = _mapper.Map<List<UserProjectDTO>>(userProjects);
            _response.StatusCode = HttpStatusCode.OK;
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
    /// EndPoint <c>GetProjectById<\c> get a project by his id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}", Name = "GetProjectById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetProject(Guid id)
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


            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id, includeProperties: "Address");
            if (project == null)
            {
                _response.Messages.Add("This project doesn't exists.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            ProjectDTO projectDTO = _mapper.Map<ProjectDTO>(project);
            List<UserProject> userProject = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == projectDTO.Id, includeProperties: "User");
            List<UserProjectDTO> userProjectDTO = _mapper.Map<List<UserProjectDTO>>(userProject);
            foreach (var item in userProjectDTO)
            {
                User user = await _unitOfWork.Users.GetAsync(u => u.Id == item.UserId.ToString());
                UserProjectRoleDTO userDTO = _mapper.Map<UserProjectRoleDTO>(user);
                userDTO.Role = item.Role;
                projectDTO.Users.Add(userDTO);
            }


            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = projectDTO;
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
    ///  EndPoint <c>CreateProject<\c> Creates a project and assigns the creator as the owner of the project
    /// </summary>
    /// <param name="createProjectDTO"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> CreateProject([FromBody] CreateProjectDTO createProjectDTO)
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
                UserId = loggedUser.Id,
                ProjectId = newProject.Id,
                Role = "owner"
            };

            await _unitOfWork.UsersProjects.CreateAsync(newUserProject);


            ProjectDTO projectDTO = _mapper.Map<ProjectDTO>(newProject);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Messages.Add("The Project was created with success");
            _response.Result = projectDTO;

            return CreatedAtRoute("GetProjectById", new { id = newProject.Id }, _response);
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
    /// EndPoint <c>DeleteProject<\c> Delete the project if the user is the owner
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}", Name = "DeleteProject")]
    public async Task<ActionResult<APIResponse>> DeleteProject(Guid id)
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

            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id);
            if (project == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages!.Add($"Project with {id} was not found");
                return NotFound(_response);
            }

            var projectUsers = await _unitOfWork.UsersProjects.GetAsync(r => r.ProjectId == project.Id && r.UserId == loggedUser.Id);
            if (projectUsers == null || projectUsers.Role != "owner")
            {
                _response.Messages.Add("You are not the owner of the project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Forbidden;
                return StatusCode(StatusCodes.Status403Forbidden, _response);
            }

            // remove address
            project.Address = await _unitOfWork.Addresses.GetAsync(u => u.Id == project.AddressId, false);
            await _unitOfWork.Projects.RemoveAsync(project);
            await _unitOfWork.Addresses.RemoveAsync(project.Address);

            _response.Messages.Add("The Project was deleted");
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





}