using Microsoft.EntityFrameworkCore;
using Questlog.Application.Auth;
using Questlog.Application.Common;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly QuestlogDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(QuestlogDbContext db, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw AppException.Conflict("Email already registered.");
        if (await _db.Users.AnyAsync(u => u.Username == username, ct))
            throw AppException.Conflict("Username already taken.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = _hasher.Hash(request.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(_jwt.CreateToken(user), user.Id, user.Username);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var key = request.EmailOrUsername.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Email == key || u.Username == request.EmailOrUsername.Trim(), ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw AppException.Unauthorized("Invalid credentials.");

        return new AuthResponse(_jwt.CreateToken(user), user.Id, user.Username);
    }
}
