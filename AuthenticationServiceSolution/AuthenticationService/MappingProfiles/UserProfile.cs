using AuthenticationService.DTOs;
using AuthenticationService.Models;
using AutoMapper;

namespace AuthenticationService.MappingProfiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        // Source -> Target
        CreateMap<User, UserDto>().ReverseMap();

        CreateMap<IEnumerable<User>, IEnumerable<UserDto>>().ReverseMap();
    }
}
