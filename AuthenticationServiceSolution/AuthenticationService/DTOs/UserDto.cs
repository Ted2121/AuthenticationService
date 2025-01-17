﻿using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs;

public class UserDto
{
    public string? Id { get; set; }
    [Required]
    public string? UserName { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    [Required]
    public string? Email { get; set; }
}
