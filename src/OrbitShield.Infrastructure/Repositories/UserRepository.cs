using Microsoft.EntityFrameworkCore;
using OrbitShield.Application.Repositories;
using OrbitShield.Domain.Entities;
using OrbitShield.Infrastructure.Persistence;

namespace OrbitShield.Infrastructure.Repositories;

public sealed class UserRepository(OrbitShieldDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await dbContext.Users.AddAsync(user, cancellationToken);
}
