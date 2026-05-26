using Microsoft.EntityFrameworkCore;
using OrbitShield.Application.Repositories;
using OrbitShield.Domain.Entities;
using OrbitShield.Infrastructure.Persistence;

namespace OrbitShield.Infrastructure.Repositories;

public sealed class SatelliteRepository(OrbitShieldDbContext dbContext) : ISatelliteRepository
{
    public async Task<IReadOnlyList<Satellite>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Satellites
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

    public Task<Satellite?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Satellites.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Satellite?> GetDefaultAsync(CancellationToken cancellationToken = default) =>
        dbContext.Satellites.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.Satellites.AnyAsync(x => x.Code == code, cancellationToken);

    public async Task AddAsync(Satellite satellite, CancellationToken cancellationToken = default) =>
        await dbContext.Satellites.AddAsync(satellite, cancellationToken);

    public void Remove(Satellite satellite) => dbContext.Satellites.Remove(satellite);
}
