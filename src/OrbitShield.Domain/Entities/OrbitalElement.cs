namespace OrbitShield.Domain.Entities;

public sealed class OrbitalElement
{
    public int Id { get; set; }
    public int SatelliteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TleLine1 { get; set; } = string.Empty;
    public string TleLine2 { get; set; } = string.Empty;
    public DateTime Epoch { get; set; }
    public string Source { get; set; } = "CelesTrak";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Satellite? Satellite { get; set; }
}
