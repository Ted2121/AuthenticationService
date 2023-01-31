using AuthenticationService.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationService.Helpers;

/// <summary>
/// To be used by both UsersController and AdminsController
/// </summary>
public static class JwtTokenCreationHelper
{
   
    public static string CreateToken(User user, IConfiguration configuration)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        };


        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Users:JwtToken"]));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            // TODO : replace 30 days with 15 mins for production
            // expires: DateTime.Now.AddMinutes(15),
            expires: DateTime.Now.AddDays(30),
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}
