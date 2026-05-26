namespace OrbitShield.Domain.Entities;

public sealed class Satellite
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OrbitStatus { get; set; } = OrbitStatuses.Nominal;
    public int FuelPercentage { get; set; }
    public int SolarEnergyPercentage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrbitalElement> OrbitalElements { get; set; } = [];
    public ICollection<ConjunctionEvent> ConjunctionEvents { get; set; } = [];
    public ICollection<ManeuverLog> ManeuverLogs { get; set; } = [];
    public ICollection<SensorReading> SensorReadings { get; set; } = [];
}
