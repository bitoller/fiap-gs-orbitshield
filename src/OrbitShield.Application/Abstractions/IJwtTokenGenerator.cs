using OrbitShield.Domain.Entities;

namespace OrbitShield.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}
