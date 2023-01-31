using AuthenticationService.Data;
using AuthenticationService.DTOs;
using AuthenticationService.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdminsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    private readonly IConfiguration _configuration;

    public AdminsController(IUserRepository userRepository, IMapper mapper, ILogger<UsersController> logger, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<string>> CreateUserAsync(UserDto userDto, string adminKey)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if(!adminKey.Equals(_configuration["Admins:AdminKey"]))
        {
            return Unauthorized("Invalid admin key");
        }

        userDto.Id = Guid.NewGuid().ToString();
        var userModel = _mapper.Map<User>(userDto);
        userModel.Role = "Admin";
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
}
