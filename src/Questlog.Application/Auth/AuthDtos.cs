namespace Questlog.Application.Auth;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string EmailOrUsername, string Password);
public record AuthResponse(string Token, Guid UserId, string Username);
