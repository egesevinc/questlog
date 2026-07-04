using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Questlog.Application.Common;

namespace Questlog.Api.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UserId
    {
        get
        {
            var sub = _accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => UserId is not null;
}
