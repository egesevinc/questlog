using System.ComponentModel.DataAnnotations;

namespace Questlog.Application.Auth;

public record RegisterRequest(
    [Required, StringLength(32, MinimumLength = 2)] string Username,
    [Required, EmailAddress, StringLength(256)] string Email,
    // 72 is BCrypt's effective byte limit; anything longer is silently truncated.
    [Required, StringLength(72, MinimumLength = 8)] string Password);

public record LoginRequest(
    [Required] string EmailOrUsername,
    [Required] string Password);

public record AuthResponse(string Token, Guid UserId, string Username);
