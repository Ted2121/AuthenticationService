using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models;

public class User
{
    [Key]
    public string Id { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsOwner { get; set; }
}
