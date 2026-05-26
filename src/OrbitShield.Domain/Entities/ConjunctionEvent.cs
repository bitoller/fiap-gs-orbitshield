namespace OrbitShield.Domain.Entities;

public sealed class ConjunctionEvent
{
    public int Id { get; set; }
    public int SatelliteId { get; set; }
    public int DebrisObjectId { get; set; }
    public decimal DistanceKm { get; set; }
    public decimal Probability { get; set; }
    public bool CollisionRisk { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = ConjunctionStatuses.Open;

    public Satellite? Satellite { get; set; }
    public DebrisObject? DebrisObject { get; set; }
    public ICollection<ManeuverLog> ManeuverLogs { get; set; } = [];
}
