using Microsoft.AspNetCore.Authorization;

namespace AuthenticationService.Authorization;

public class OnlyOwnerRequirement : IAuthorizationRequirement
{

}