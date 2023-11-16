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

            if (!ModelState.IsValid)
            {
                _response.Messages.Add("Your request needs to have a Id.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }


            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id, includeProperties: "Address");

            if (project == null)
            {
                _response.Messages.Add("You aren't part of a project.");
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
                _response.Messages!.Add("The project with ID was not found");
                return NotFound(_response);
            }

            var projectUsers = await _unitOfWork.UsersProjects.GetAllAsync(r => r.ProjectId == project.Id);
            var userPermission = projectUsers.FirstOrDefault(u => u.UserId == loggedUser.Id);
            if (userPermission == null || userPermission.Role != "owner")
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

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.Messages.Add("The Project was deleted");
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
    /// Update self
    /// </summary>
    /// <param name="updateUserFromProjectDTO"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("Users/Update", Name = "UpdateUserFromProject")]
    public async Task<ActionResult<APIResponse>> UpdateUserFromProject([FromBody] UpdateUserFromProjectDTO updateUserFromProjectDTO)
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


            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == updateUserFromProjectDTO.ProjectId);
            if (project == null)
            {
                _response.Messages.Add("A project with this ID does not exists.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }


            List<UserProject> userProject = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == project.Id);
            var loggedUserRole = userProject.FirstOrDefault(u => u.UserId == loggedUser.Id);
            // user with role worker in a project can't add others
            if (loggedUserRole == null || loggedUserRole.Role == "worker")
            {
                _response.Messages.Add("You are not part of the management of this project.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            var userToUpdate = await _userManager.FindByIdAsync(updateUserFromProjectDTO.UserId);
            if (userToUpdate == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages.Add("User with this Id doesn't exist.");
                return NotFound(_response);
            }

            var userToUpdateRole = userProject.FirstOrDefault(u => u.UserId == userToUpdate.Id);
            if (userToUpdateRole == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages.Add("This user is not in this project.");
                return NotFound(_response);
            }

            if (loggedUserRole.Role == "manager" && userToUpdateRole.Role == "owner")
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.IsSuccess = false;
                _response.Messages.Add("A manager can't change the owner information.");
                return Unauthorized(_response);
            }

            if (updateUserFromProjectDTO.File != null)
            {
                var photoPath = await UploadFile(updateUserFromProjectDTO.File);
                userToUpdate.PhotoPath = photoPath;
            }


            await _userManager.UpdateAsync(userToUpdate);
            _response.Messages.Add("the user was updated with success.");
            _response.StatusCode = HttpStatusCode.NoContent;
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
    /// EndPoint <c>AddNewUser<\c> add a new user to the project with a role
    /// </summary>
    /// <param name="createNewUserProjectDTO"></param>
    /// <returns></returns>
    [HttpPost("Users/Create", Name = "AddNewUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> AddNewUser([FromForm] CreateNewUserProjectDTO createNewUserProjectDTO)
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


            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == createNewUserProjectDTO.ProjectId);
            if (project == null)
            {
                _response.Messages.Add("A project with this ID does not exists.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }


            List<UserProject> userProject = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == project.Id);
            var loggedUserRole = userProject.FirstOrDefault(u => u.UserId == loggedUser.Id);
            // user with role worker in a project can't add others
            if (loggedUserRole == null || loggedUserRole.Role == "worker")
            {
                _response.Messages.Add("You are not part of the management of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            var userExists = await _userManager.FindByEmailAsync(createNewUserProjectDTO.Email);
            if (userExists != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Messages.Add("User with email already exists");
                return BadRequest(_response);
            }

            var newUser = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Email = createNewUserProjectDTO.Email,
                Name = createNewUserProjectDTO.FirstName + " " + createNewUserProjectDTO.LastName,
                UserName = createNewUserProjectDTO.UserName ?? createNewUserProjectDTO.Email,
                PhotoPath = createNewUserProjectDTO.PhotoPath ?? string.Empty,
            };

            await _userManager.CreateAsync(newUser, GeneratePassword());

            UserProject relationship = new()
            {
                Role = createNewUserProjectDTO.Role,
                Project = project,
                ProjectId = project.Id,
                User = newUser,
                UserId = newUser.Id
            };
            await _unitOfWork.UsersProjects.CreateAsync(relationship);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<UserProjectDTO>(relationship);
            _response.Messages.Add("The user was created and added to the project with success.");
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


    [HttpDelete("Users/Delete", Name = "DeleteUserFromProject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> DeleteUserFromProject([FromForm] DeleteUserFromProjectDTO deleteUserFromProjectDTO)
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


            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == deleteUserFromProjectDTO.ProjectId);
            if (project == null)
            {
                _response.Messages.Add("A project with this ID does not exists.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            // check who is trying to delete
            List<UserProject> usersProject = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == project.Id);
            var loggedUserRole = usersProject.FirstOrDefault(u => u.UserId == loggedUser.Id);
            // user with role worker in a project can't add others
            if (loggedUserRole == null || loggedUserRole.Role == "worker")
            {
                _response.Messages.Add("You are not part of the management of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            var userToDelete = await _userManager.FindByIdAsync(deleteUserFromProjectDTO.UserId);
            if (userToDelete == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages.Add("User with this Id doesn't exist.");
                return NotFound(_response);
            }

            // check the user to be deleted
            var userToDeleteRole = usersProject.FirstOrDefault(u => u.UserId == userToDelete.Id);
            // user with role worker in a project can't add others
            if (userToDeleteRole == null)
            {
                _response.Messages.Add("This user is not in this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            if (userToDeleteRole.Role == "owner")
            {
                _response.Messages.Add("You can't delete the owner of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            await _userManager.DeleteAsync(userToDelete);

            _response.StatusCode = HttpStatusCode.OK;
            _response.Messages.Add("User deleted with success.");
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
    /// EndPoint <c>AddUser<\c> add a existing user to the project with a role
    /// </summary>
    /// <param name="createUserProjectDTO"></param>
    /// <returns></returns>
    [HttpPost("Users", Name = "AddUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> AddUser([FromBody] CreateUserProjectDTO createUserProjectDTO)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _response.Messages.Add("Your request is missing one or more parameter.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedUser == null)
            {
                _response.Messages.Add("You are not logged in.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            var project = await _unitOfWork.Projects.GetAsync(u => u.Id == createUserProjectDTO.ProjectId);
            if (project == null)
            {
                _response.Messages.Add("A project with this ID does not exists.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            List<UserProject> userProject = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == project.Id);
            var loggedUserRole = userProject.FirstOrDefault(u => u.UserId == loggedUser.Id);
            /// user with role "worker" can't add others
            if (loggedUserRole == null || loggedUserRole.Role == "worker")
            {
                _response.Messages.Add("You are not part of thes management of this project");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }

            foreach (var relation in userProject)
            {
                if (relation.UserId == createUserProjectDTO.UserId.ToString())
                {
                    _response.Messages.Add("This user is in this project already.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.Forbidden;
                    return StatusCode(StatusCodes.Status403Forbidden, _response);
                }
            }


            UserProject relationship = _mapper.Map<UserProject>(createUserProjectDTO);
            await _unitOfWork.UsersProjects.CreateAsync(relationship);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<UserProjectDTO>(relationship);
            _response.Messages.Add("The user was successfully added to the project.");
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
    /// EndPoint <c>RemoveUser<\c> remove a existing user from a project
    /// </summary>
    /// <param name="UserProjectDeleteDTO"></param>
    /// <returns></returns>
    [HttpDelete("Users", Name = "RemoveUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> RemoveUser([FromBody] RemoveUserProjectDTO UserProjectDeleteDTO)
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



            List<UserProject> userProjects = await _unitOfWork.UsersProjects.GetAllAsync(u => u.ProjectId == UserProjectDeleteDTO.ProjectId);
            var loggedUserRole = userProjects.FirstOrDefault(u => u.UserId == loggedUser.Id);
            /// user with role "worker" can't remove others
            /// unless he's removing him self
            if (loggedUserRole == null)
            {
                _response.Messages.Add("You are not part of this project.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Forbidden;
                return StatusCode(StatusCodes.Status403Forbidden, _response);
            }
            /// user with role "owner can't remove self
            if (loggedUserRole.Role == "owner" && UserProjectDeleteDTO.UserId == loggedUser.Id)
            {
                _response.Messages.Add("Can't delete the owner of the project.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Forbidden;
                return StatusCode(StatusCodes.Status403Forbidden, _response);
            }
            var userProjectToDelete = userProjects.FirstOrDefault(u => u.UserId == UserProjectDeleteDTO.UserId);
            if (userProjectToDelete == null)
            {
                _response.Messages.Add("This user is not in this project.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            if (loggedUserRole.Role == "Manager" && userProjectToDelete.Role == "owner")
            {
                _response.Messages.Add("Can't delete the owner of the project.");
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Forbidden;
                return StatusCode(StatusCodes.Status403Forbidden, _response);
            }
            UserProject relationship = _mapper.Map<UserProject>(userProjectToDelete);
            await _unitOfWork.UsersProjects.RemoveAsync(relationship);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = _mapper.Map<UserProjectDTO>(relationship);
            _response.Messages.Add("The user was removed with success.");
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
    /// Generate a random password for the new user
    /// </summary>
    /// <returns></returns>
    [NonAction]
    private string GeneratePassword()
    {
        Random res = new Random();

        // String of alphabets  
        string alphabet = "abcdefghijklmnopqrstuvwxyz";
        int size = 10;

        // Initializing the empty string 
        string newPass = "";

        for (int i = 0; i < size; i++)
        {
            // Selecting a index randomly 
            int x = res.Next(26);

            // Appending the character at the  
            // index to the random string. 
            newPass += alphabet[x];
        }
        return newPass;
    }

}