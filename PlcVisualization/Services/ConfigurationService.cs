using Microsoft.EntityFrameworkCore;
using PlcVisualization.Data;
using PlcVisualization.Models;

namespace PlcVisualization.Services
{
    /// <summary>
    /// Service für die Verwaltung von Antrieb-Konfigurationen
    /// </summary>
    public class ConfigurationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly Dictionary<int, DriveConfiguration> _configCache = new();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);
        private DriveLoggingService? _loggingService;

        public ConfigurationService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<ConfigurationService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Setzt den LoggingService (wird nach Initialisierung aufgerufen, um zirkuläre Abhängigkeit zu vermeiden)
        /// </summary>
        public void SetLoggingService(DriveLoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        /// <summary>
        /// Lädt alle Antrieb-Konfigurationen
        /// </summary>
        public async Task<List<DriveConfiguration>> GetAllConfigurationsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveConfigurations
                .OrderBy(d => d.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Lädt eine einzelne Konfiguration (mit Caching)
        /// </summary>
        public async Task<DriveConfiguration?> GetConfigurationAsync(int driveId)
        {
            // Cache prüfen
            if (_configCache.ContainsKey(driveId) &&
                DateTime.UtcNow - _lastCacheUpdate < _cacheLifetime)
            {
                return _configCache[driveId];
            }

            // Aus Datenbank laden
            using var context = await _contextFactory.CreateDbContextAsync();
            var config = await context.DriveConfigurations
                .FirstOrDefaultAsync(d => d.Id == driveId);

            if (config != null)
            {
                _configCache[driveId] = config;
            }

            return config;
        }

        /// <summary>
        /// Lädt alle Konfigurationen in den Cache
        /// </summary>
        public async Task LoadAllIntoCache()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var configs = await context.DriveConfigurations.ToListAsync();

            _configCache.Clear();
            foreach (var config in configs)
            {
                _configCache[config.Id] = config;
            }

            _lastCacheUpdate = DateTime.UtcNow;
            _logger.LogInformation($"Loaded {configs.Count} drive configurations into cache");
        }

        /// <summary>
        /// Speichert eine Konfiguration
        /// </summary>
        public async Task<bool> SaveConfigurationAsync(DriveConfiguration configuration)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var existing = await context.DriveConfigurations
                    .FirstOrDefaultAsync(d => d.Id == configuration.Id);

                if (existing != null)
                {
                    // Update
                    existing.Name = configuration.Name;
                    existing.Description = configuration.Description;
                    existing.Group = configuration.Group;
                    existing.HasForward = configuration.HasForward;
                    existing.HasReverse = configuration.HasReverse;
                    existing.HasSetpoint = configuration.HasSetpoint;
                    existing.HasSpeedDisplay = configuration.HasSpeedDisplay;
                    existing.HasCurrentDisplay = configuration.HasCurrentDisplay;
                    existing.HasErrorDisplay = configuration.HasErrorDisplay;
                    existing.MinSetpoint = configuration.MinSetpoint;
                    existing.MaxSetpoint = configuration.MaxSetpoint;
                    existing.DefaultSetpoint = configuration.DefaultSetpoint;
                    existing.LayoutX = configuration.LayoutX;
                    existing.LayoutY = configuration.LayoutY;
                    existing.Color = configuration.Color;
                    existing.IsActive = configuration.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;

                    context.DriveConfigurations.Update(existing);
                }
                else
                {
                    // Insert
                    context.DriveConfigurations.Add(configuration);
                }

                await context.SaveChangesAsync();

                // Cache aktualisieren
                _configCache[configuration.Id] = configuration;

                // Konfigurationsänderung loggen
                if (_loggingService != null)
                {
                    await _loggingService.LogConfigChangeAsync(configuration.Id,
                        $"Konfiguration aktualisiert: Name='{configuration.Name}', Gruppe='{configuration.Group}'");
                }

                _logger.LogInformation($"Saved configuration for drive {configuration.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving configuration for drive {configuration.Id}");
                return false;
            }
        }

        /// <summary>
        /// Setzt eine Konfiguration auf Standardwerte zurück
        /// </summary>
        public async Task<bool> ResetToDefaultAsync(int driveId)
        {
            var defaultConfig = new DriveConfiguration
            {
                Id = driveId,
                Name = $"Antrieb {driveId}",
                Description = null,
                Group = driveId <= 50 ? "Gruppe A" : "Gruppe B",
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
                UpdatedAt = DateTime.UtcNow
            };

            return await SaveConfigurationAsync(defaultConfig);
        }

        /// <summary>
        /// Gibt alle aktiven Antriebe zurück
        /// </summary>
        public async Task<List<DriveConfiguration>> GetActiveConfigurationsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveConfigurations
                .Where(d => d.IsActive)
                .OrderBy(d => d.Id)
                .ToListAsync();
        }
    }
}
