using AuthenticationService.Data;
using AuthenticationService.DTOs;
using AuthenticationService.Helpers;
using AuthenticationService.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using User = AuthenticationService.Models.User;

namespace AuthenticationService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "OnlyOwner")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    private readonly IConfiguration _configuration;

    public UsersController(IUserRepository userRepository, IMapper mapper, ILogger<UsersController> logger, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }



    [Route("{id}")]
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetUserByIdAsync(string id)
    {
   
        var user = await _userRepository.GetUserByIdAsync(id);
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
        var userModel = _mapper.Map<User>(userDto);
        userModel.Role = "User";
        userModel.IsOwner = true;
        var returnedId = await _userRepository.CreateUserAsync(userModel);

        if (returnedId == null)
        {
            return BadRequest();
        }

        if (returnedId.ToLower().Equals("username already exists"))
        {
            return BadRequest("Username already exists");
        }

        return Ok(returnedId);
    }

    //This id is used in authorization policy
    [Route("{id}")]
    [HttpPut]
    public async Task<ActionResult> UpdateUserAync(string id, UserDto userDto)
    {
        userDto.Id = id;
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
    public async Task<ActionResult<string>> LoginUserAsync(UserLoginDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (userDto == null)
        {
            return BadRequest();
        }

        // We need this to be able to retrieve the user with the role property
        var userModel = await _userRepository.GetUserByUserameAsync(userDto.UserName);
        if(userModel == null)
        {
            return NotFound();
        }

        var authenticatedUser = _userRepository.Login(userDto.UserName, userDto.Password);

        if (authenticatedUser == null)
        {
            return Unauthorized();
        }

        var token = JwtTokenCreationHelper.CreateToken(userModel, _configuration);

        return Ok(token);
    }


    // Id is used in authorization policy
    [Route("updatepassword/{id}")]
    [HttpPut]
    public async Task<ActionResult> UpdatePasswordAsync(string id, UserDto userDto)
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
