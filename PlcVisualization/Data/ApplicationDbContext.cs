using Microsoft.EntityFrameworkCore;
using PlcVisualization.Models;

namespace PlcVisualization.Data
{
    /// <summary>
    /// Entity Framework DbContext für die Anwendungs-Datenbank
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Antrieb-Konfigurationen
        /// </summary>
        public DbSet<DriveConfiguration> DriveConfigurations { get; set; }

        /// <summary>
        /// Antrieb-Protokoll (Historie & Logging)
        /// </summary>
        public DbSet<DriveLog> DriveLogs { get; set; }

        /// <summary>
        /// Layout-Konfigurationen
        /// </summary>
        public DbSet<LayoutConfiguration> LayoutConfigurations { get; set; }

        /// <summary>
        /// Antriebspositionen in Layouts
        /// </summary>
        public DbSet<DriveLayoutPosition> DriveLayoutPositions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Index für schnellere Suche nach ID
            modelBuilder.Entity<DriveConfiguration>()
                .HasIndex(d => d.Id)
                .IsUnique();

            // Index für Gruppe (für zukünftige Filterung)
            modelBuilder.Entity<DriveConfiguration>()
                .HasIndex(d => d.Group);

            // Indizes für DriveLog (für schnelle Abfragen)
            modelBuilder.Entity<DriveLog>()
                .HasIndex(d => d.DriveId);

            modelBuilder.Entity<DriveLog>()
                .HasIndex(d => d.Timestamp);

            modelBuilder.Entity<DriveLog>()
                .HasIndex(d => d.EventType);

            modelBuilder.Entity<DriveLog>()
                .HasIndex(d => new { d.DriveId, d.Timestamp });

            // Indizes für LayoutConfiguration
            modelBuilder.Entity<LayoutConfiguration>()
                .HasIndex(l => l.IsDefault);

            modelBuilder.Entity<LayoutConfiguration>()
                .HasIndex(l => l.SortOrder);

            // Indizes für DriveLayoutPosition
            modelBuilder.Entity<DriveLayoutPosition>()
                .HasIndex(p => p.LayoutId);

            modelBuilder.Entity<DriveLayoutPosition>()
                .HasIndex(p => p.DriveId);

            modelBuilder.Entity<DriveLayoutPosition>()
                .HasIndex(p => new { p.LayoutId, p.DriveId })
                .IsUnique();

            // Beziehungen
            modelBuilder.Entity<DriveLayoutPosition>()
                .HasOne(p => p.Layout)
                .WithMany(l => l.DrivePositions)
                .HasForeignKey(p => p.LayoutId)
                .OnDelete(DeleteBehavior.Cascade);

            // Standard-Layout erstellen
            modelBuilder.Entity<LayoutConfiguration>().HasData(
                new LayoutConfiguration
                {
                    Id = 1,
                    Name = "Standard-Layout",
                    Description = "Automatisch generiertes Grid-Layout",
                    Width = 1920,
                    Height = 1080,
                    GridSize = 20,
                    IsDefault = true,
                    IsActive = true,
                    SortOrder = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Standard-Werte für alle 100 Antriebe beim Erstellen der Datenbank
            var configurations = new List<DriveConfiguration>();
            for (int i = 1; i <= 100; i++)
            {
                configurations.Add(new DriveConfiguration
                {
                    Id = i,
                    Name = $"Antrieb {i}",
                    Description = null,
                    Group = i <= 50 ? "Gruppe A" : "Gruppe B", // Beispiel-Gruppierung
                    HasForward = true,
                    HasReverse = true,
                    HasSetpoint = true,
                    HasSpeedDisplay = true,
                    HasCurrentDisplay = true,
                    HasErrorDisplay = true,
                    MinSetpoint = 0,
                    MaxSetpoint = 3000,
                    DefaultSetpoint = 1500,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            modelBuilder.Entity<DriveConfiguration>().HasData(configurations);
        }
    }
}
