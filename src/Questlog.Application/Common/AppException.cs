namespace Questlog.Application.Common;

/// <summary>Thrown for expected, client-correctable errors (mapped to 4xx).</summary>
public class AppException : Exception
{
    public int StatusCode { get; }
    public AppException(string message, int statusCode = 400) : base(message) => StatusCode = statusCode;

    public static AppException NotFound(string what) => new($"{what} not found.", 404);
    public static AppException Conflict(string message) => new(message, 409);
    public static AppException Unauthorized(string message = "Not authenticated.") => new(message, 401);
}
