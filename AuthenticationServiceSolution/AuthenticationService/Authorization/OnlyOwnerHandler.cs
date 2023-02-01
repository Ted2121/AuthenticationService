using AuthenticationService.Data;
using AuthenticationService.Models;
using Microsoft.AspNetCore.Authorization;

namespace AuthenticationService.Authorization;

public class OnlyOwnerHandler : AuthorizationHandler<OnlyOwnerRequirement>
{
    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public OnlyOwnerHandler(AppDbContext appDbContext, IHttpContextAccessor contextAccessor)
    {
        _appDbContext = appDbContext;
        _contextAccessor = contextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlyOwnerRequirement requirement)
    {
        var userId = context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
        var user = _appDbContext.Users.SingleOrDefault(u => u.Id == userId);
        var path = _contextAccessor.HttpContext.Request.Path;
        
        var resourceId = path.Value.Split('/').Last().ToString();

        if (user != null && user.Id == resourceId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
