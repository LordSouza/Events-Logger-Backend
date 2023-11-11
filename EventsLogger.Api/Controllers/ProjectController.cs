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
using System.Net;

namespace EventsLogger.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetProjects()
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

            IEnumerable<UserProject> userProjects = await _unitOfWork.UsersProjects.GetAllAsync(u => u.UserId == profile.Id, includeProperties: "Project");
            _response.Result = _mapper.Map<List<UserProjectDTO>>(userProjects);
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

    [HttpPost("Users/add", Name = "AddUser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> AddUser([FromBody] CreateUserProjectDTO createUserProjectDTO)
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

            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == createUserProjectDTO.ProjectId);
            if (project == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            List<UserProject> userProject = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == project.Id);
            var loggedUserRole = userProject.FirstOrDefault(u => u.UserId == loggedUser.Id);
            /// user with role "worker" can't remove others
            /// unless he's removing him self
            if (loggedUserRole == null || loggedUserRole.Role == "worker")
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            if (userProject != null)
            {
                foreach (var relation in userProject)
                {
                    if (relation.UserId == createUserProjectDTO.UserId.ToString())
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.Messages.Add("User Already in this project");
                        return BadRequest(_response);
                    }
                }
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

    [HttpDelete("Users/remove", Name = "RemoveUser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> RemoveUser([FromBody] UpdateUserProjectDTO UserProjectDeleteDTO)
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

            List<UserProject> userProjects = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == UserProjectDeleteDTO.ProjectId);
            var loggedUserRole = userProjects.FirstOrDefault(u => u.UserId == loggedUser.Id);
            /// user with role "worker" can't remove others
            /// unless he's removing him self
            if (loggedUserRole == null || loggedUserRole.Role == "worker" && UserProjectDeleteDTO.UserId != loggedUser.Id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            /// user with role "owner can't remove self
            if (loggedUserRole.Role == "owner" && UserProjectDeleteDTO.UserId == loggedUser.Id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            var userProjectToDelete = userProjects.FirstOrDefault(u => u.UserId == UserProjectDeleteDTO.UserId);
            if (userProjectToDelete == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            if (loggedUserRole.Role == "Manager" && userProjectToDelete.Role == "owner")
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            UserProject relationship = _mapper.Map<UserProject>(userProjectToDelete);
            await _unitOfWork.UsersProjects.RemoveAsync(relationship);

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




    [HttpGet("{id:guid}", Name = "GetProjectById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetProject(Guid id)
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



            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id, includeProperties: "Address");

            if (project == null)
            {
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


            ProjectDTO projectDTO = _mapper.Map<ProjectDTO>(newProject);



            _response.StatusCode = HttpStatusCode.Created;
            _response.Messages.Add("The Project was created with success");
            _response.Result = projectDTO;

            return CreatedAtRoute("GetProjectById", new { id = newProject.Id }, _response);
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
    [HttpDelete("{id:guid}", Name = "DeleteProject")]
    public async Task<ActionResult<APIResponse>> DeleteProject(Guid id)
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

            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id);
            if (project == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages!.Add("The project with ID was not found");
                return NotFound(_response);
            }

            var projectUsers = await _unitOfWork.UsersProjects.GetAllAsync(r => r.ProjectId == project.Id);
            var userPermission = projectUsers.FirstOrDefault(u => u.UserId == loggedUser.Id);
            if (userPermission == null || userPermission.Role != "owner")
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }

            // remove address
            project.Address = await _unitOfWork.Addresses.GetAsync(u => u.Id == project.AddressId, false);
            await _unitOfWork.Projects.RemoveAsync(project);
            await _unitOfWork.Addresses.RemoveAsync(project.Address);

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.Messages!.Add("The Project was deleted");
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.Messages = new List<string> { ex.ToString() };
        }
        return _response;
    }


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