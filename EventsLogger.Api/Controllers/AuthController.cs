using AutoMapper;
using EventsLogger.Api.Configuration;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace EventsLogger.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly APIResponse _response;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly JwtConfig _jwtConfig;
    public AuthController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, IConfiguration configuration, IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork, mapper)
    {
        _jwtConfig = optionsMonitor.CurrentValue;
        _configuration = configuration;
        _userManager = userManager;
        _response = new();
    }

    // [HttpGet]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<APIResponse>> GetEntries()
    // {
    //     try
    //     {
    //         IEnumerable<User> UserList = await _unitOfWork.Users.GetAllAsync();
    //         _response.Result = _mapper.Map<List<UserDTO>>(UserList);
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



    // [HttpGet("{id:guid}", Name = "GetUser")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<APIResponse>> GetUser(Guid id)
    // {
    //     try
    //     {
    //         var User = await _unitOfWork.Users.GetAsync(u => u.Id == id.ToString());
    //         if (User == null)
    //         {
    //             _response.StatusCode = HttpStatusCode.NotFound;
    //             return NotFound(_response);
    //         }
    //         _response.StatusCode = HttpStatusCode.OK;
    //         _response.Result = _mapper.Map<UserDTO>(User);
    //         return Ok(_response);
    //     }
    //     catch (Exception ex)
    //     {
    //         _response.IsSuccess = false;
    //         _response.Messages = new List<string> { ex.ToString() };
    //     }
    //     return _response;
    // }


    [HttpPost]
    [Route("Register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> Register([FromBody] UserRegistrationRequestDTO requestDTO)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Messages.Add("Invalid request");
                return BadRequest(_response);
            }

            var userExists = await _userManager.FindByEmailAsync(requestDTO.Email);
            if (userExists != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Messages.Add("Username with email already exists");
                return BadRequest(_response);
            }

            var newUser = new User()
            {
                Email = requestDTO.Email,
                Name = requestDTO.Name,
                UserName = requestDTO.Name,

            };

            var isCreated = await _userManager.CreateAsync(newUser, requestDTO.Password);

            if (!isCreated.Succeeded)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = isCreated.Errors.Select(x => x.Description).ToList();
                return BadRequest(_response);
            }
            // token

            var token = GenerateJwtToken(newUser);


            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = token;
            return Created("GetUser", _response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.Messages.Add(ex.ToString());
        }
        return _response;
    }


    [HttpPost]
    [Route("Login")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> Login([FromBody] UserLoginRequestDTO requestDTO)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Messages.Add("Invalid request");
                return BadRequest(_response);
            }

            var existingUser = await _userManager.FindByEmailAsync(requestDTO.Email);
            if (existingUser == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Messages.Add("Invalid authentication");
                return BadRequest(_response);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(existingUser, requestDTO.Password);


            if (!isPasswordValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Messages.Add("Invalid authentication");
                return BadRequest(_response);
            }
            // token

            var token = GenerateJwtToken(existingUser);

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = token;
            return Created("GetUser", _response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.Messages.Add(ex.ToString());
        }
        return _response;
    }


    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}", Name = "DeleteUser")]
    public async Task<ActionResult<APIResponse>> DeleteUser(Guid id)
    {
        try
        {

            var User = await _unitOfWork.Users.GetAsync(u => u.Id == id.ToString());
            if (User == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            await _unitOfWork.Users.RemoveAsync(User);

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
    [HttpPut("{id:guid}", Name = "UpdateUser")]
    public async Task<ActionResult<APIResponse>> UpdateUser(Guid id, [FromBody] UpdateUserDTO updateUserDTO)
    {
        try
        {

            if (updateUserDTO == null || id != updateUserDTO.Id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            User user = _mapper.Map<User>(updateUserDTO);

            await _unitOfWork.Users.UpdateAsync(user);
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

    [HttpPatch("{id:guid}", Name = "UpdatePartialUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> UpdatePartialUser(Guid id, JsonPatchDocument<UpdateUserDTO> patchDTO)
    {
        try
        {

            var User = await _unitOfWork.Users.GetAsync(u => u.Id == id.ToString(), false);

            if (User is null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            UpdateUserDTO UserDTO = _mapper.Map<UpdateUserDTO>(User);

            patchDTO.ApplyTo(UserDTO, ModelState);

            User model = _mapper.Map<User>(UserDTO);

            await _unitOfWork.Users.UpdateAsync(model);

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


    // TODO SEND TO THE REPOSITORY
    private string GenerateJwtToken(User user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

        // Token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())
            }),
            Expires = DateTime.Now.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(token);
    }

}