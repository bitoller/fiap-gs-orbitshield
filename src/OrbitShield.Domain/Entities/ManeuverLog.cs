namespace OrbitShield.Domain.Entities;

public sealed class ManeuverLog
{
    public int Id { get; set; }
    public int SatelliteId { get; set; }
    public int? ConjunctionEventId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int ServoAngle { get; set; }
    public decimal? ThrustLevel { get; set; }
    public string Source { get; set; } = "ESP32";
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    public Satellite? Satellite { get; set; }
    public ConjunctionEvent? ConjunctionEvent { get; set; }
}
