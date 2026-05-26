namespace OrbitShield.Domain.Entities;

public sealed class DebrisObject
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal EstimatedSizeMeters { get; set; }
    public string RiskLevel { get; set; } = RiskLevels.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ConjunctionEvent> ConjunctionEvents { get; set; } = [];
}
