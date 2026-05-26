using OrbitShield.Application.Abstractions;

namespace OrbitShield.Infrastructure.Persistence;

public sealed class UnitOfWork(OrbitShieldDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
