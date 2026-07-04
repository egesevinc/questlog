namespace Questlog.Application.Common;

/// <summary>
/// Abstraction over "who is making this request", so application services don't
/// depend on ASP.NET's HttpContext directly. Keeps the Application layer
/// framework-agnostic and testable.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
