// using AutoMapper;
// using EventsLogger.DataService.Repositories.Interfaces;
// using EventsLogger.Entities.DbSet;
// using EventsLogger.Entities.Dtos.Requests;
// using EventsLogger.Entities.Dtos.Response;
// using Microsoft.AspNetCore.JsonPatch;
// using Microsoft.AspNetCore.Mvc;
// using System.Net;

// namespace EventsLogger.Api.Controllers;

// //[Route("api/[controller]")]
// [Route("api/Project")]
// [ApiController]
// public class ProjectAPIController : BaseController
// {
//     private readonly APIResponse _response;

//     public ProjectAPIController(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
//     {
//         _response = new();
//     }

//     [HttpGet]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     public async Task<ActionResult<APIResponse>> GetProjects()
//     {
//         try
//         {
//             IEnumerable<Project> ProjectList = await _unitOfWork.Projects.GetAllAsync(includeProperties: "Address");

//             _response.Result = _mapper.Map<List<ProjectDTO>>(ProjectList);
//             _response.StatusCode = HttpStatusCode.OK;
//             return Ok(_response);
//         }
//         catch (Exception ex)
//         {
//             _response.IsSuccess = false;
//             _response.Messages = new List<string> { ex.ToString() };
//         }
//         return _response;
//     }

//     [HttpPost("AddUser/{id:guid}", Name = "AddUser")]
//     [ProducesResponseType(StatusCodes.Status201Created)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<ActionResult<APIResponse>> AddUser(Guid id, [FromBody] CreateUserProjectDTO createUserProjectDTO)
//     {
//         try
//         {
//             var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id, includeProperties: "Address");
//             if (project == null)
//             {
//                 _response.StatusCode = HttpStatusCode.NotFound;
//                 return NotFound(_response);
//             }
//             UserProject relationship = _mapper.Map<UserProject>(createUserProjectDTO);
//             await _unitOfWork.UsersProjects.CreateAsync(relationship);

//             _response.StatusCode = HttpStatusCode.Created;
//             _response.Result = _mapper.Map<UserProjectDTO>(relationship);

//             return Ok(_response);
//         }
//         catch (Exception ex)
//         {
//             _response.IsSuccess = false;
//             _response.Messages = new List<string> { ex.ToString() };
//         }
//         return _response;
//     }




//     [HttpGet("{id:guid}", Name = "GetProject")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<ActionResult<APIResponse>> GetProject(Guid id)
//     {
//         try
//         {
//             var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id);

//             var address = await _unitOfWork.Addresses.GetAsync(u => u.Id == project.AddressId);


//             AddressDTO addressDTO = new()
//             {
//                 Id = address.Id,
//                 City = address.City,
//                 State = address.State,
//                 Street = address.Street,
//                 ZipCode = address.ZipCode,
//                 Country = address.Country,
//             };

//             ProjectDTO projectDTO = new()
//             {
//                 CreatedDate = project.CreatedDate,
//                 UpdatedDate = project.UpdatedDate,
//                 Id = project.Id,
//                 Name = project.Name,
//                 Address = addressDTO,
//                 AddressId = address.Id,
//                 CreatorId = project.CreatorId
//             };

//             if (project == null)
//             {
//                 _response.StatusCode = HttpStatusCode.NotFound;
//                 return NotFound(_response);
//             }
//             _response.StatusCode = HttpStatusCode.OK;
//             _response.Result = projectDTO;
//             return Ok(_response);
//         }
//         catch (Exception ex)
//         {
//             _response.IsSuccess = false;
//             _response.Messages = new List<string> { ex.ToString() };
//         }
//         return _response;
//     }


//     [HttpPost]
//     [ProducesResponseType(StatusCodes.Status201Created)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//     public async Task<ActionResult<APIResponse>> CreateProject([FromBody] CreateProjectDTO createProjectDTO)
//     {
//         try
//         {
//             if (createProjectDTO == null)
//             {
//                 _response.StatusCode = HttpStatusCode.BadRequest;
//                 return BadRequest(_response);
//             }

//             // TODO refactor the DTO mapping

//             Address address = new()
//             {
//                 Id = Guid.NewGuid(),
//                 State = createProjectDTO.Address.State,
//                 Street = createProjectDTO.Address.Street,
//                 City = createProjectDTO.Address.City,
//                 ZipCode = createProjectDTO.Address.ZipCode,
//                 Country = createProjectDTO.Address.Country,
//             };
//             await _unitOfWork.Addresses.CreateAsync(address);

//             var user = await _unitOfWork.Users.GetAsync(u => u.Id == createProjectDTO.CreatorId);
//             if (user is null)
//             {
//                 _response.Messages!.Add("Invalid User");
//                 _response.StatusCode = HttpStatusCode.BadRequest;
//                 return BadRequest(_response);
//             }


//             Project project = new()
//             {
//                 Id = Guid.NewGuid(),
//                 Name = createProjectDTO.Name,
//                 CreatedDate = DateTime.UtcNow,
//                 Address = address,
//                 AddressId = address.Id,
//                 CreatorId = user.Id,
//                 Creator = user,
//             };
//             await _unitOfWork.Projects.CreateAsync(project);


//             AddressDTO addressDTO = new()
//             {
//                 Id = address.Id,
//                 City = address.City,
//                 State = address.State,
//                 Street = address.Street,
//                 ZipCode = address.ZipCode,
//                 Country = address.Country,
//             };

//             ProjectDTO projectDTO = new()
//             {
//                 CreatedDate = project.CreatedDate,
//                 UpdatedDate = project.UpdatedDate,
//                 Id = project.Id,
//                 Name = project.Name,
//                 Address = addressDTO,
//                 AddressId = addressDTO.Id,
//                 CreatorId = project.CreatorId
//             };


//             _response.StatusCode = HttpStatusCode.Created;
//             _response.Messages.Add("The Project was created with success");
//             _response.Result = projectDTO;

//             return CreatedAtRoute("GetProject", new { id = project.Id }, _response);
//         }
//         catch (Exception ex)
//         {
//             _response.IsSuccess = false;
//             _response.Messages = new List<string> { ex.ToString() };
//         }
//         return _response;
//     }


//     [ProducesResponseType(StatusCodes.Status204NoContent)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [HttpDelete("{id:guid}", Name = "DeleteProject")]
//     public async Task<ActionResult<APIResponse>> DeleteProject(Guid id)
//     {
//         try
//         {

//             var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id);
//             if (project == null)
//             {
//                 _response.StatusCode = HttpStatusCode.NotFound;
//                 _response.IsSuccess = false;
//                 _response.Messages!.Add("The project with ID was not found");
//                 return NotFound(_response);
//             }
//             // remove address
//             project.Address = await _unitOfWork.Addresses.GetAsync(u => u.Id == project.AddressId, false);
//             //await _unitOfWork.Addresses.RemoveAsync(project.Address);
//             await _unitOfWork.Projects.RemoveAsync(project);

//             _response.StatusCode = HttpStatusCode.NoContent;
//             _response.Messages!.Add("The Project was deleted");
//             _response.IsSuccess = true;
//             return Ok(_response);
//         }
//         catch (Exception ex)
//         {
//             _response.StatusCode = HttpStatusCode.InternalServerError;
//             _response.IsSuccess = false;
//             _response.Messages = new List<string> { ex.ToString() };
//         }
//         return _response;
//     }


//     [HttpPatch("{id:guid}", Name = "UpdatePartialProject")]
//     [ProducesResponseType(StatusCodes.Status204NoContent)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     public async Task<ActionResult<APIResponse>> UpdatePartialProject(Guid id, JsonPatchDocument<UpdateProjectDTO> patchDTO)
//     {
//         try
//         {

//             if (id == Guid.Empty)
//             {
//                 _response.StatusCode = HttpStatusCode.BadRequest;
//                 _response.Messages!.Add("The Id is invalid");
//                 _response.IsSuccess = false;
//                 return BadRequest(_response);
//             }
//             var project = await _unitOfWork.Projects.GetAsync(u => u.Id == id, false);

//             if (project == null)
//             {
//                 _response.StatusCode = HttpStatusCode.BadRequest;
//                 _response.Messages!.Add("The project doesn't exists");
//                 _response.IsSuccess = false;
//                 return BadRequest(_response);
//             }

//             UpdateProjectDTO projectDTO = _mapper.Map<UpdateProjectDTO>(project);

//             patchDTO.ApplyTo(projectDTO, ModelState);

//             Project model = _mapper.Map<Project>(projectDTO);



//             if (patchDTO.Operations[0].path == "/address")
//             {
//                 Address address = new()
//                 {
//                     Id = model.AddressId,
//                     State = model.Address.State,
//                     Street = model.Address.Street,
//                     City = model.Address.City,
//                     ZipCode = model.Address.ZipCode,
//                     Country = model.Address.Country,
//                 };
//                 await _unitOfWork.Addresses.UpdateAsync(address);

//             }
//             else
//             {
//                 model.Address = await _unitOfWork.Addresses.GetAsync(u => u.Id == model.AddressId, false);
//                 await _unitOfWork.Projects.UpdateAsync(model);
//             }


//             if (!ModelState.IsValid)
//             {
//                 _response.StatusCode = HttpStatusCode.BadRequest;
//                 _response.IsSuccess = false;
//                 return BadRequest(_response);
//             }
//             _response.StatusCode = HttpStatusCode.NoContent;
//             _response.Messages!.Add("The patch was applied with success");
//             _response.IsSuccess = true;
//             return Ok(_response);
//         }
//         catch (Exception ex)
//         {
//             _response.StatusCode = HttpStatusCode.InternalServerError;
//             _response.IsSuccess = false;
//             _response.Messages = new List<string> { ex.ToString() };
//         }
//         return _response;
//     }

// }