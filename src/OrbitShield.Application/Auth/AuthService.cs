using OrbitShield.Application.Abstractions;
using OrbitShield.Application.Common;
using OrbitShield.Application.Repositories;
using OrbitShield.Domain;
using OrbitShield.Domain.Entities;

namespace OrbitShield.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork) : IAuthService
{
    private static readonly HashSet<string> AllowedRoles = [UserRoles.Admin, UserRoles.Engineer, UserRoles.SatelliteDevice];

    public async Task<OperationResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return OperationResult.Fail("Name, email and password are required.");
        }

        if (request.Password.Length < 8)
        {
            return OperationResult.Fail("Password must contain at least 8 characters.");
        }

        if (!AllowedRoles.Contains(request.Role))
        {
            return OperationResult.Fail("Invalid role.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return OperationResult.Fail("Email is already registered.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("User registered successfully.");
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var (token, expiresAt) = jwtTokenGenerator.Generate(user);
        return new AuthResponse(
            token,
            "Bearer",
            expiresAt,
            new UserProfileResponse(user.Id, user.Name, user.Email, user.Role));
    }
}
