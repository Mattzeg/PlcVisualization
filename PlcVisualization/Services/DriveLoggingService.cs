using Microsoft.EntityFrameworkCore;
using PlcVisualization.Data;
using PlcVisualization.Models;
using System.Text.Json;

namespace PlcVisualization.Services
{
    /// <summary>
    /// Service für die Protokollierung aller Antriebsereignisse
    /// </summary>
    public class DriveLoggingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<DriveLoggingService> _logger;
        private readonly ConfigurationService _configService;

        public DriveLoggingService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<DriveLoggingService> logger,
            ConfigurationService configService)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _configService = configService;
        }

        /// <summary>
        /// Protokolliert ein Kommando an einen Antrieb
        /// </summary>
        public async Task LogCommandAsync(int driveId, DriveCommand command, string? user = null)
        {
            var config = await _configService.GetConfigurationAsync(driveId);
            var description = BuildCommandDescription(command);

            var log = new DriveLog
            {
                DriveId = driveId,
                DriveName = config?.Name ?? $"Antrieb {driveId}",
                Timestamp = DateTime.UtcNow,
                EventType = DriveEventType.Command,
                Description = description,
                User = user,
                AdditionalData = JsonSerializer.Serialize(command)
            };

            await SaveLogAsync(log);
        }

        /// <summary>
        /// Protokolliert eine Zustandsänderung
        /// </summary>
        public async Task LogStateChangeAsync(int driveId, string property, string oldValue, string newValue)
        {
            var config = await _configService.GetConfigurationAsync(driveId);

            var log = new DriveLog
            {
                DriveId = driveId,
                DriveName = config?.Name ?? $"Antrieb {driveId}",
                Timestamp = DateTime.UtcNow,
                EventType = DriveEventType.StateChange,
                Description = $"{property} geändert",
                OldValue = oldValue,
                NewValue = newValue
            };

            await SaveLogAsync(log);
        }

        /// <summary>
        /// Protokolliert einen Fehler
        /// </summary>
        public async Task LogErrorAsync(int driveId, int errorCode)
        {
            var config = await _configService.GetConfigurationAsync(driveId);

            var log = new DriveLog
            {
                DriveId = driveId,
                DriveName = config?.Name ?? $"Antrieb {driveId}",
                Timestamp = DateTime.UtcNow,
                EventType = DriveEventType.Error,
                Description = $"Fehler aufgetreten: Code {errorCode}",
                ErrorCode = errorCode
            };

            await SaveLogAsync(log);
        }

        /// <summary>
        /// Protokolliert dass ein Fehler behoben wurde
        /// </summary>
        public async Task LogErrorClearedAsync(int driveId, int previousErrorCode)
        {
            var config = await _configService.GetConfigurationAsync(driveId);

            var log = new DriveLog
            {
                DriveId = driveId,
                DriveName = config?.Name ?? $"Antrieb {driveId}",
                Timestamp = DateTime.UtcNow,
                EventType = DriveEventType.ErrorCleared,
                Description = $"Fehler {previousErrorCode} behoben",
                ErrorCode = previousErrorCode
            };

            await SaveLogAsync(log);
        }

        /// <summary>
        /// Protokolliert eine Konfigurationsänderung
        /// </summary>
        public async Task LogConfigChangeAsync(int driveId, string description, string? user = null)
        {
            var config = await _configService.GetConfigurationAsync(driveId);

            var log = new DriveLog
            {
                DriveId = driveId,
                DriveName = config?.Name ?? $"Antrieb {driveId}",
                Timestamp = DateTime.UtcNow,
                EventType = DriveEventType.ConfigChanged,
                Description = description,
                User = user
            };

            await SaveLogAsync(log);
        }

        /// <summary>
        /// Gibt alle Log-Einträge zurück
        /// </summary>
        public async Task<List<DriveLog>> GetAllLogsAsync(int pageSize = 1000, int pageNumber = 1)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Gibt Log-Einträge für einen bestimmten Antrieb zurück
        /// </summary>
        public async Task<List<DriveLog>> GetLogsByDriveAsync(int driveId, int pageSize = 500, int pageNumber = 1)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveLogs
                .Where(l => l.DriveId == driveId)
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Gibt Log-Einträge nach Event-Typ zurück
        /// </summary>
        public async Task<List<DriveLog>> GetLogsByEventTypeAsync(string eventType, int pageSize = 500, int pageNumber = 1)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveLogs
                .Where(l => l.EventType == eventType)
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Gibt Log-Einträge in einem Zeitraum zurück
        /// </summary>
        public async Task<List<DriveLog>> GetLogsByDateRangeAsync(DateTime from, DateTime to, int? driveId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.DriveLogs
                .Where(l => l.Timestamp >= from && l.Timestamp <= to);

            if (driveId.HasValue)
            {
                query = query.Where(l => l.DriveId == driveId.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Gibt alle Fehler-Einträge zurück
        /// </summary>
        public async Task<List<DriveLog>> GetErrorLogsAsync(int pageSize = 500, int pageNumber = 1)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveLogs
                .Where(l => l.EventType == DriveEventType.Error)
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Löscht alte Log-Einträge (älter als X Tage)
        /// </summary>
        public async Task<int> DeleteOldLogsAsync(int daysToKeep)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldLogs = await context.DriveLogs
                .Where(l => l.Timestamp < cutoffDate)
                .ToListAsync();

            context.DriveLogs.RemoveRange(oldLogs);
            await context.SaveChangesAsync();

            return oldLogs.Count;
        }

        /// <summary>
        /// Gibt die Gesamtanzahl der Log-Einträge zurück
        /// </summary>
        public async Task<int> GetLogCountAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveLogs.CountAsync();
        }

        private async Task SaveLogAsync(DriveLog log)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.DriveLogs.Add(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Speichern des Logs für Antrieb {log.DriveId}");
            }
        }

        private string BuildCommandDescription(DriveCommand command)
        {
            var parts = new List<string>();

            if (command.Start.HasValue)
                parts.Add($"Start={command.Start.Value}");
            if (command.Stop.HasValue)
                parts.Add($"Stop={command.Stop.Value}");
            if (command.Forward.HasValue)
                parts.Add($"Forward={command.Forward.Value}");
            if (command.Reverse.HasValue)
                parts.Add($"Reverse={command.Reverse.Value}");
            if (command.Setpoint.HasValue)
                parts.Add($"Setpoint={command.Setpoint.Value}");
            if (command.ModeAuto.HasValue)
                parts.Add($"ModeAuto={command.ModeAuto.Value}");

            return $"Kommando: {string.Join(", ", parts)}";
        }
    }
}
