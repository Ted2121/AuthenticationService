using AuthenticationService.Data;
using AuthenticationService.DTOs;
using AuthenticationService.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger = null;
    private readonly IConfiguration _configuration;

    public UsersController(IUserRepository userRepository, IMapper mapper, ILogger<UsersController> logger, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        if (users == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<UserDto>(users));
    }

    [Route("{id}")]
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetUserByIdAsync(string id)
    {
        var user = _userRepository.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<UserDto>(user));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<string>> CreateUserAsync(UserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        userDto.Id = Guid.NewGuid().ToString();
        var returnedId = await _userRepository.CreateUserAsync(_mapper.Map<User>(userDto));
        if (returnedId == null)
        {
            return BadRequest();
        }

        return Ok(returnedId);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUserAync(UserDto userDto)
    {
        if (!await _userRepository.UpdateUserAsync(_mapper.Map<User>(userDto)))
        {
            return BadRequest();
        }

        return Ok();
    }

    [Route("{id}")]
    [HttpDelete]
    public async Task<ActionResult> DeleteUserAsync(string id)
    {
        if (await _userRepository.GetUserByIdAsync(id) == null)
        {
            return NotFound();
        }

        if (!await _userRepository.DeleteUserAsync(id))
        {
            return BadRequest();
        }

        return NoContent();
    }

    [AllowAnonymous]
    [Route("login")]
    [HttpPost]
    public ActionResult<string> LoginUserAsync(UserLoginDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (userDto == null)
        {
            return BadRequest();
        }

        var userModel = _mapper.Map<User>(userDto);


        var authenticatedUser = _userRepository.Login(userModel.UserName, userModel.Password);

        if (authenticatedUser == null)
        {
            return Unauthorized();
        }

        var token = CreateToken(userModel);

        return Ok(token);
    }

    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Users:JwtToken"]));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        
        var token = new JwtSecurityToken(
            claims: claims,
            // TODO : replace 30 days with 15 mins for production
            // expires: DateTime.Now.AddMinutes(15),
            expires: DateTime.Now.AddDays(30),
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    [Route("updatepassword")]
    [HttpPut]
    public async Task<ActionResult> UpdatePasswordAsync(UserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userRepository.GetUserByIdAsync(userDto.Id);

        if (user == null)
        {
            return NotFound();
        }

        if (!await _userRepository.UpdateUserAsync(_mapper.Map<User>(userDto)))
        {
            return BadRequest();
        }

        return NoContent();
    }

}
