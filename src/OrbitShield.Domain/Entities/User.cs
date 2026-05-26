namespace OrbitShield.Domain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.Engineer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
