using Microsoft.EntityFrameworkCore;
using OrbitShield.Domain;
using OrbitShield.Domain.Entities;

namespace OrbitShield.Infrastructure.Persistence;

public sealed class OrbitShieldDbContext(DbContextOptions<OrbitShieldDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Satellite> Satellites => Set<Satellite>();
    public DbSet<DebrisObject> DebrisObjects => Set<DebrisObject>();
    public DbSet<ConjunctionEvent> ConjunctionEvents => Set<ConjunctionEvent>();
    public DbSet<ManeuverLog> ManeuverLogs => Set<ManeuverLog>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<OrbitalElement> OrbitalElements => Set<OrbitalElement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("OS_USERS", table =>
            {
                table.HasCheckConstraint("CK_OS_USERS_ROLE", "ROLE IN ('Admin', 'Engineer', 'SatelliteDevice')");
            });
            entity.HasKey(x => x.Id).HasName("PK_OS_USERS");
            entity.Property(x => x.Id).HasColumnName("USER_ID").UseIdentityColumn();
            entity.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasColumnName("EMAIL").HasMaxLength(180).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("PASSWORD_HASH").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Role).HasColumnName("ROLE").HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
            entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UK_OS_USERS_EMAIL");
        });

        modelBuilder.Entity<Satellite>(entity =>
        {
            entity.ToTable("OS_SATELLITES", table =>
            {
                table.HasCheckConstraint("CK_OS_SATELLITES_FUEL", "FUEL_PERCENTAGE BETWEEN 0 AND 100");
                table.HasCheckConstraint("CK_OS_SATELLITES_SOLAR", "SOLAR_ENERGY_PERCENTAGE BETWEEN 0 AND 100");
                table.HasCheckConstraint("CK_OS_SATELLITES_STATUS", "ORBIT_STATUS IN ('Nominal', 'Warning', 'Emergency')");
            });
            entity.HasKey(x => x.Id).HasName("PK_OS_SATELLITES");
            entity.Property(x => x.Id).HasColumnName("SATELLITE_ID").UseIdentityColumn();
            entity.Property(x => x.Code).HasColumnName("CODE").HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
            entity.Property(x => x.OrbitStatus).HasColumnName("ORBIT_STATUS").HasMaxLength(30).IsRequired();
            entity.Property(x => x.FuelPercentage).HasColumnName("FUEL_PERCENTAGE").IsRequired();
            entity.Property(x => x.SolarEnergyPercentage).HasColumnName("SOLAR_ENERGY_PERCENTAGE").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
            entity.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UK_OS_SATELLITES_CODE");
        });

        modelBuilder.Entity<DebrisObject>(entity =>
        {
            entity.ToTable("OS_DEBRIS_OBJECTS", table =>
            {
                table.HasCheckConstraint("CK_OS_DEBRIS_SIZE", "ESTIMATED_SIZE_METERS >= 0");
                table.HasCheckConstraint("CK_OS_DEBRIS_RISK", "RISK_LEVEL IN ('Low', 'Medium', 'High', 'Critical')");
            });
            entity.HasKey(x => x.Id).HasName("PK_OS_DEBRIS_OBJECTS");
            entity.Property(x => x.Id).HasColumnName("DEBRIS_OBJECT_ID").UseIdentityColumn();
            entity.Property(x => x.Code).HasColumnName("CODE").HasMaxLength(60).IsRequired();
            entity.Property(x => x.EstimatedSizeMeters).HasColumnName("ESTIMATED_SIZE_METERS").HasPrecision(8, 2).IsRequired();
            entity.Property(x => x.RiskLevel).HasColumnName("RISK_LEVEL").HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
            entity.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UK_OS_DEBRIS_OBJECTS_CODE");
        });

        modelBuilder.Entity<ConjunctionEvent>(entity =>
        {
            entity.ToTable("OS_CONJUNCTION_EVENTS", table =>
            {
                table.HasCheckConstraint("CK_OS_CONJ_DISTANCE", "DISTANCE_KM >= 0");
                table.HasCheckConstraint("CK_OS_CONJ_PROBABILITY", "PROBABILITY BETWEEN 0 AND 100");
                table.HasCheckConstraint("CK_OS_CONJ_STATUS", "STATUS IN ('Open', 'Mitigated', 'Closed')");
            });
            entity.HasKey(x => x.Id).HasName("PK_OS_CONJUNCTION_EVENTS");
            entity.Property(x => x.Id).HasColumnName("CONJUNCTION_EVENT_ID").UseIdentityColumn();
            entity.Property(x => x.SatelliteId).HasColumnName("SATELLITE_ID").IsRequired();
            entity.Property(x => x.DebrisObjectId).HasColumnName("DEBRIS_OBJECT_ID").IsRequired();
            entity.Property(x => x.DistanceKm).HasColumnName("DISTANCE_KM").HasPrecision(10, 2).IsRequired();
            entity.Property(x => x.Probability).HasColumnName("PROBABILITY").HasPrecision(5, 2).IsRequired();
            entity.Property(x => x.CollisionRisk).HasColumnName("COLLISION_RISK").HasConversion<int>().IsRequired();
            entity.Property(x => x.DetectedAt).HasColumnName("DETECTED_AT").IsRequired();
            entity.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Satellite).WithMany(x => x.ConjunctionEvents).HasForeignKey(x => x.SatelliteId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_OS_CONJ_SATELLITE");
            entity.HasOne(x => x.DebrisObject).WithMany(x => x.ConjunctionEvents).HasForeignKey(x => x.DebrisObjectId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_OS_CONJ_DEBRIS");
        });

        modelBuilder.Entity<ManeuverLog>(entity =>
        {
            entity.ToTable("OS_MANEUVER_LOGS", table =>
            {
                table.HasCheckConstraint("CK_OS_MANEUVER_SERVO", "SERVO_ANGLE BETWEEN 0 AND 180");
            });
            entity.HasKey(x => x.Id).HasName("PK_OS_MANEUVER_LOGS");
            entity.Property(x => x.Id).HasColumnName("MANEUVER_LOG_ID").UseIdentityColumn();
            entity.Property(x => x.SatelliteId).HasColumnName("SATELLITE_ID").IsRequired();
            entity.Property(x => x.ConjunctionEventId).HasColumnName("CONJUNCTION_EVENT_ID");
            entity.Property(x => x.Action).HasColumnName("ACTION").HasMaxLength(240).IsRequired();
            entity.Property(x => x.ServoAngle).HasColumnName("SERVO_ANGLE").IsRequired();
            entity.Property(x => x.ThrustLevel).HasColumnName("THRUST_LEVEL").HasPrecision(8, 2);
            entity.Property(x => x.Source).HasColumnName("SOURCE").HasMaxLength(40).IsRequired();
            entity.Property(x => x.ExecutedAt).HasColumnName("EXECUTED_AT").IsRequired();
            entity.HasOne(x => x.Satellite).WithMany(x => x.ManeuverLogs).HasForeignKey(x => x.SatelliteId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_OS_MANEUVER_SATELLITE");
            entity.HasOne(x => x.ConjunctionEvent).WithMany(x => x.ManeuverLogs).HasForeignKey(x => x.ConjunctionEventId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_OS_MANEUVER_CONJUNCTION");
        });

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("OS_SENSOR_READINGS", table =>
            {
                table.HasCheckConstraint("CK_OS_SENSOR_BATTERY", "BATTERY_VOLTAGE >= 0");
                table.HasCheckConstraint("CK_OS_SENSOR_RADIATION", "RADIATION_LEVEL >= 0");
            });
            entity.HasKey(x => x.Id).HasName("PK_OS_SENSOR_READINGS");
            entity.Property(x => x.Id).HasColumnName("SENSOR_READING_ID").UseIdentityColumn();
            entity.Property(x => x.SatelliteId).HasColumnName("SATELLITE_ID").IsRequired();
            entity.Property(x => x.TemperatureCelsius).HasColumnName("TEMPERATURE_CELSIUS").HasPrecision(8, 2).IsRequired();
            entity.Property(x => x.RadiationLevel).HasColumnName("RADIATION_LEVEL").HasPrecision(8, 2).IsRequired();
            entity.Property(x => x.BatteryVoltage).HasColumnName("BATTERY_VOLTAGE").HasPrecision(8, 2).IsRequired();
            entity.Property(x => x.SimulatedGravity).HasColumnName("SIMULATED_GRAVITY").HasPrecision(8, 4);
            entity.Property(x => x.SimulatedThrust).HasColumnName("SIMULATED_THRUST").HasPrecision(8, 2);
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
            entity.HasOne(x => x.Satellite).WithMany(x => x.SensorReadings).HasForeignKey(x => x.SatelliteId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_OS_SENSOR_SATELLITE");
        });

        modelBuilder.Entity<OrbitalElement>(entity =>
        {
            entity.ToTable("OS_ORBITAL_ELEMENTS", table =>
            {
                table.HasCheckConstraint("CK_OS_TLE_LINE1", "TLE_LINE1 LIKE '1 %'");
                table.HasCheckConstraint("CK_OS_TLE_LINE2", "TLE_LINE2 LIKE '2 %'");
            });
            entity.HasKey(x => x.Id).HasName("PK_OS_ORBITAL_ELEMENTS");
            entity.Property(x => x.Id).HasColumnName("ORBITAL_ELEMENT_ID").UseIdentityColumn();
            entity.Property(x => x.SatelliteId).HasColumnName("SATELLITE_ID").IsRequired();
            entity.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
            entity.Property(x => x.TleLine1).HasColumnName("TLE_LINE1").HasMaxLength(120).IsRequired();
            entity.Property(x => x.TleLine2).HasColumnName("TLE_LINE2").HasMaxLength(120).IsRequired();
            entity.Property(x => x.Epoch).HasColumnName("EPOCH").IsRequired();
            entity.Property(x => x.Source).HasColumnName("SOURCE").HasMaxLength(60).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
            entity.HasOne(x => x.Satellite).WithMany(x => x.OrbitalElements).HasForeignKey(x => x.SatelliteId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_OS_TLE_SATELLITE");
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Satellite>().HasData(new Satellite
        {
            Id = 1,
            Code = "ORB-01",
            Name = "Orbit Shield Sentinel",
            OrbitStatus = OrbitStatuses.Nominal,
            FuelPercentage = 82,
            SolarEnergyPercentage = 94,
            CreatedAt = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
