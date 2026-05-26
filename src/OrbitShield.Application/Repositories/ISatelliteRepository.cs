using OrbitShield.Domain.Entities;

namespace OrbitShield.Application.Repositories;

public interface ISatelliteRepository
{
    Task<IReadOnlyList<Satellite>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Satellite?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Satellite?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Satellite satellite, CancellationToken cancellationToken = default);
    void Remove(Satellite satellite);
}
