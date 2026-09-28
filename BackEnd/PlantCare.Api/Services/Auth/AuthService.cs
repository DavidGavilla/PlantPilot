using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Users;
using PlantCare.Api.Mappers;
using PlantCare.Api.Models;
using PlantCare.Api.Models.Workspaces;
using PlantCare.Api.Services.Interfaces.Auth;

namespace PlantCare.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(AppDbContext context, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        var emailTaken = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == dto.Email, cancellationToken);

        if (emailTaken)
            return AuthResult.EmailTaken();

        var user = dto.ToModel();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var workspace = new Workspace
        {
            Type = WorkspaceType.Personal,
            Name = $"{dto.Name} {dto.LastName} (personal)"
        };
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync(cancellationToken);

        var membership = new WorkspaceMember
        {
            WorkspaceId = workspace.WorkspaceId,
            UserId = user.UserId,
            Role = WorkspaceRole.Owner
        };
        _context.WorkspaceMembers.Add(membership);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);
        var (refreshToken, refreshTokenExpiresAt) = await _refreshTokenService.IssueAsync(user.UserId, cancellationToken);

        return AuthResult.Ok(token, refreshToken, refreshTokenExpiresAt, user.ToDto());
    }

    public async Task<AuthResult> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        // Same generic failure whether the email doesn't exist or the password is wrong — never let
        // a client distinguish the two (user-enumeration prevention).
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return AuthResult.InvalidCredentials();

        var token = _jwtTokenService.GenerateToken(user);
        var (refreshToken, refreshTokenExpiresAt) = await _refreshTokenService.IssueAsync(user.UserId, cancellationToken);

        return AuthResult.Ok(token, refreshToken, refreshTokenExpiresAt, user.ToDto());
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        return user?.ToDto();
    }
}
