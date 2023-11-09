using AutoMapper;
using EventsLogger.Api.Configuration;
using EventsLogger.BlobService.Repositories.Interfaces;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests;
using EventsLogger.Entities.Dtos.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    private readonly JwtConfig _jwtConfig;
    public AuthController(
        IOptionsMonitor<JwtConfig> optionsMonitor,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<User> userManager,
        IBlobManagement blobManagement,
        IQueuesManagement queuesManagement,
        IConfiguration configuration) : base(
            unitOfWork,
            mapper,
            userManager,
            blobManagement,
            queuesManagement,
            configuration)
    {
        _jwtConfig = optionsMonitor.CurrentValue;
        _response = new();
    }


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
                Name = requestDTO.FirstName + " " + requestDTO.LastName,
                UserName = requestDTO.UserName

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

            AuthResult authResult = new()
            {
                Token = token
            };
            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = authResult;
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

            AuthResult authResult = new()
            {
                Token = token
            };
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = authResult;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.IsSuccess = false;
            _response.Messages.Add(ex.ToString());
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
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())
            }),
            Expires = DateTime.Now.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(token);
    }

}