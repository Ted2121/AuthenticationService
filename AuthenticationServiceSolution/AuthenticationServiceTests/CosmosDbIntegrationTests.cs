using AuthenticationService.Data;
using AuthenticationService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationServiceTests;
public class CosmosDbIntegrationTests
{
    private DbContextOptionsBuilder<AppDbContext> _dbContextOptionsBuilder;
    private AppDbContext _appDbContext;
    private IUserRepository _userRepository;
    private User _user;
    private IConfiguration _configuration;


    public CosmosDbIntegrationTests()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<CosmosDbIntegrationTests>();
        _configuration = builder.Build();
    }

    #region Configuration
    private void CreateUser()
    {
        _user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "Bob",
            Password = "Pass",
            FirstName = "Bobby",
            LastName = "Singer",
            Email = "Bobby@gmail.com"
        };
    }
    private void InitializeDependencies()
    {
        _appDbContext = new AppDbContext(_dbContextOptionsBuilder.Options);
        _userRepository = new UserRepository(_appDbContext);
    }
    private void ConfigureDbContext()
    {
        _dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        _dbContextOptionsBuilder.UseCosmos(
            _configuration["Users:AccountEndpoint"],
            _configuration["Users:AccountKey"],
            _configuration["Users:Database"]
            );
    }
    #endregion

    [SetUp]
    public void SetUp()
    {
        CreateUser();
        ConfigureDbContext();
        InitializeDependencies();
    }


    [TearDown]
    public void TearDown()
    {
        _userRepository.DeleteUserAsync(_user.Id);
        _appDbContext.Dispose();

    }

    [Test]
    public async Task TestingCreateUserAsyncExpectingNotNullIdReturned()
    {
        // Arrange in SetUp

        // Act
        var userId = await _userRepository.CreateUserAsync(_user);


        // Assert
        Assert.That(userId, Is.Not.Null);
    }

    [Test]
    public async Task TestingDeleteUserAsyncExpectingTrue()
    {
        // Arrange
        await _userRepository.CreateUserAsync(_user);

        // Act
        var isDeleted = await _userRepository.DeleteUserAsync(_user.Id);


        // Assert
        Assert.That(isDeleted, Is.True);
    }

    [Test]
    public async Task TestingGetAllUsersAsyncExpectingAnyReturned()
    {
        // Arrange
        await _userRepository.CreateUserAsync(_user);

        // Act
        var users = await _userRepository.GetAllUsersAsync();

        // Assert
        Assert.That(users.Any, Is.True);
    }

    [Test]
    public async Task TestingGetUserByIdExpectingUserInstanceReturned()
    {
        // Arrange
        await _userRepository.CreateUserAsync(_user);

        // Act
        var user = await _userRepository.GetUserByIdAsync(_user.Id);

        // Assert
        Assert.That(user, Is.Not.Null);
    }

    [Test]
    public async Task TestingUpdateUserAsyncExpectingChangesSaved()
    {
        // Arrange
        await _userRepository.CreateUserAsync(_user);

        // Act
        _user.Email = "test@test";
        var isUpdated = await _userRepository.UpdateUserAsync(_user);

        // Assert
        Assert.That(isUpdated, Is.True);
    }


    [Test]
    public async Task TestingLoggingInWithCorrectInformationExpectingUserInstanceReturned()
    {
        // Arrange
        await _userRepository.CreateUserAsync(_user);

        // Act
        var user = _userRepository.Login(_user.UserName, _user.Password);

        // Assert
        Assert.That(_user, Is.Not.Null);
    }

    [Test]
    public async Task TestingUpdatePasswordAsyncExpectingChangesSaved()
    {
        // Arrange
        await _userRepository.CreateUserAsync(_user);

        // Act
        var isPasswordUpdated = await _userRepository.UpdatePasswordAsync(_user.UserName, _user.Password, "NewPassword");

        // Assert
        Assert.That(isPasswordUpdated, Is.True);
    }


}
