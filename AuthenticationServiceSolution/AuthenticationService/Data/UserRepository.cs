using AuthenticationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Data;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<string> CreateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == user.UserName.ToLower());
        if (existingUser != null)
        {
            return "Username already exists";
        }

        try
        {
            // This needs to be done so that password hashing doesn't affect the argument's reference
            var userToInsert = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Password = PasswordEncryption.HashPassword(user.Password),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            };

            await _appDbContext.AddAsync(userToInsert);

            var saved = await SaveChangesAsync();

            if (saved)
            {
                return user.Id;
            }
            else
            {
                throw new Exception($"User with id: {user.Id} could not be saved.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"User with id: {user.Id} could not be created. Exception was {ex.Message}");
        }
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        try
        {
            var user =
            _appDbContext.Users.FirstOrDefault(x => x.Id == id);

            _appDbContext.Remove<User>(user);
            var isDeleted = await SaveChangesAsync();

            return isDeleted;
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not delete user with id: {id}. Exception was: {ex}");
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            return await _appDbContext.Users.ToListAsync();
        }
        catch (Exception ex)
        {

            throw new Exception($"Failed getting all users. Exception was: {ex}");
        }
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        if (id == null)
        {
            throw new ArgumentNullException();
        }

        try
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));
        }
        catch (Exception ex)
        {

            throw new Exception($"Failed getting user with id: {id}. Exception was: {ex}");

        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException("User was null");
        }
        if (user.Id == null)
        {
            throw new ArgumentNullException("User's id was null");
        }

        try
        {
            var userToUpdate = await _appDbContext.FindAsync<User>(user.Id);

            if (userToUpdate == null)
            {
                throw new Exception($"User with id: {user.Id} was not found");
            }

            userToUpdate.FirstName = user.FirstName;
            userToUpdate.LastName = user.LastName;
            userToUpdate.Email = user.Email;

            return await SaveChangesAsync();
        }
        catch (Exception ex)
        {

            throw new Exception($"Failed updating user with id: {user.Id}. Exception was: {ex}");
        }

    }

    public User Login(string username, string password)
    {
        try
        {
            var user = _appDbContext.Users.Where(u => u.UserName.Equals(username)).FirstOrDefault();

            if (user != null && PasswordEncryption.ValidatePassword(password, user.Password))
            {
                return user;
            }
            else
            {
                throw new Exception($"Invalid password for user with username: {username}");
            }

        }
        catch (Exception ex)
        {

            throw new Exception($"Error logging in User with username: {username}: '{ex.Message}'.");
        }
    }

    public async Task<bool> UpdatePasswordAsync(string username, string oldPassword, string newPassword)
    {
        try
        {

            var loggedUser = Login(username, oldPassword);
            var newPasswordHashed = PasswordEncryption.HashPassword(newPassword);

            loggedUser.Password = newPasswordHashed;

            return await SaveChangesAsync();

        }
        catch (Exception ex)
        {

            throw new Exception($"Error changing password for user with username: {username}: '{ex.Message}'.");
        }
    }

    private async Task<bool> SaveChangesAsync()
    {
        return await _appDbContext.SaveChangesAsync() >= 0;
    }

    public async Task<User> GetUserByUserameAsync(string username)
    {
        if (username == null)
        {
            throw new ArgumentNullException();
        }

        try
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.UserName.Equals(username));
        }
        catch (Exception ex)
        {

            throw new Exception($"Failed getting user with username: {username}. Exception was: {ex}");

        }
    }
}
