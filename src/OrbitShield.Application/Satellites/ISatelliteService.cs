using OrbitShield.Application.Common;

namespace OrbitShield.Application.Satellites;

public interface ISatelliteService
{
    Task<IReadOnlyList<SatelliteResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SatelliteResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SatelliteResponse?> CreateAsync(CreateSatelliteRequest request, CancellationToken cancellationToken = default);
    Task<SatelliteResponse?> UpdateAsync(int id, UpdateSatelliteRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
