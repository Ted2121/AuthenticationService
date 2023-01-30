using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdminsController : ControllerBase
{
    // TODO: method for creating admin account
}
