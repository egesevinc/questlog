using Questlog.Domain.Entities;

namespace Questlog.Application.Auth;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
