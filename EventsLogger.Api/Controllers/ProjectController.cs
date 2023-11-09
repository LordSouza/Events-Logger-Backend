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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetProjects([FromQuery] bool self)
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


            if (self)
            {
                IEnumerable<UserProject> userProjects = await _unitOfWork.UsersProjects.GetAllAsync(u => u.UserId == profile.Id, includeProperties: "Project");
                _response.Result = _mapper.Map<List<UserProjectDTO>>(userProjects);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }

            IEnumerable<Project> ProjectList;
            ProjectList = await _unitOfWork.Projects.GetAllAsync();

            _response.Result = _mapper.Map<List<ProjectDTO>>(ProjectList);
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

    [HttpPost("AddUser", Name = "AddUser")]
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




    [HttpGet("GetProjectById", Name = "GetProjectById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<APIResponse>> GetProject([FromBody] ProjectGetDTO projectGetDTO)
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


            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == projectGetDTO.Id, includeProperties: "Address");

            if (project == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            ProjectDTO projectDTO = _mapper.Map<ProjectDTO>(project);
            List<UserProject> userProject = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == projectDTO.Id);
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


    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [HttpDelete("{id:guid}", Name = "DeleteProject")]
    // public async Task<ActionResult<APIResponse>> DeleteProject(Guid id)
    // {
    //     try
    //     {
    //         var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
    //         if (loggedUser == null)
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             _response.IsSuccess = false;
    //             return NotFound(_response);
    //         }



    //         var profile = await _unitOfWork.Users.GetAsync(u => u.Id == loggedUser.Id);
    //         if (profile == null)
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             _response.IsSuccess = false;
    //             return NotFound(_response);
    //         }


    //         var isPassword = await _userManager.CheckPasswordAsync(loggedUser, userPasswordRequestDTO.Password);
    //         if (!isPassword)
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             _response.IsSuccess = false;
    //             _response.Messages.Add("Wrong Password");
    //             return NotFound(_response);
    //         }

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