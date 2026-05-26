using OrbitShield.Application.Common;

namespace OrbitShield.Application.Auth;

public interface IAuthService
{
    Task<OperationResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
