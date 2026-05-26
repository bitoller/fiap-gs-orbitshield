namespace OrbitShield.Application.Auth;

public sealed record RegisterUserRequest(string Name, string Email, string Password, string Role);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    UserProfileResponse User);

public sealed record UserProfileResponse(int Id, string Name, string Email, string Role);
