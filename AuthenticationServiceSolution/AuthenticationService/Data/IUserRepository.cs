using AuthenticationService.Models;

namespace AuthenticationService.Data;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(string id);
    Task<string> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string id);
    User Login(string username, string password);
    Task<bool> UpdatePasswordAsync(string username, string oldPassword, string newPassword);

}
