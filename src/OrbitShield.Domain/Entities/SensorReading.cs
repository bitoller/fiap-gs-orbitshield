namespace OrbitShield.Domain.Entities;

public sealed class SensorReading
{
    public int Id { get; set; }
    public int SatelliteId { get; set; }
    public decimal TemperatureCelsius { get; set; }
    public decimal RadiationLevel { get; set; }
    public decimal BatteryVoltage { get; set; }
    public decimal? SimulatedGravity { get; set; }
    public decimal? SimulatedThrust { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Satellite? Satellite { get; set; }
}
