using Microsoft.EntityFrameworkCore;
using PlcVisualization.Data;
using PlcVisualization.Models;

namespace PlcVisualization.Services
{
    /// <summary>
    /// Service für die Verwaltung von Anlagenlayouts
    /// </summary>
    public class LayoutService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<LayoutService> _logger;

        public LayoutService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<LayoutService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        #region Layout-Verwaltung

        /// <summary>
        /// Gibt alle Layouts zurück
        /// </summary>
        public async Task<List<LayoutConfiguration>> GetAllLayoutsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.LayoutConfigurations
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gibt nur aktive Layouts zurück
        /// </summary>
        public async Task<List<LayoutConfiguration>> GetActiveLayoutsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.LayoutConfigurations
                .Where(l => l.IsActive)
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gibt ein Layout mit allen Positionen zurück
        /// </summary>
        public async Task<LayoutConfiguration?> GetLayoutAsync(int layoutId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.LayoutConfigurations
                .Include(l => l.DrivePositions)
                .FirstOrDefaultAsync(l => l.Id == layoutId);
        }

        /// <summary>
        /// Gibt das Standard-Layout zurück
        /// </summary>
        public async Task<LayoutConfiguration?> GetDefaultLayoutAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.LayoutConfigurations
                .Include(l => l.DrivePositions)
                .FirstOrDefaultAsync(l => l.IsDefault);
        }

        /// <summary>
        /// Erstellt ein neues Layout
        /// </summary>
        public async Task<LayoutConfiguration> CreateLayoutAsync(LayoutConfiguration layout)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Wenn neues Layout als Default markiert, alte Defaults entfernen
            if (layout.IsDefault)
            {
                await ClearDefaultFlagsAsync(context);
            }

            layout.CreatedAt = DateTime.UtcNow;
            layout.UpdatedAt = DateTime.UtcNow;

            context.LayoutConfigurations.Add(layout);
            await context.SaveChangesAsync();

            _logger.LogInformation($"Layout '{layout.Name}' (ID: {layout.Id}) erstellt");
            return layout;
        }

        /// <summary>
        /// Aktualisiert ein Layout
        /// </summary>
        public async Task<bool> UpdateLayoutAsync(LayoutConfiguration layout)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var existing = await context.LayoutConfigurations
                    .FirstOrDefaultAsync(l => l.Id == layout.Id);

                if (existing == null)
                    return false;

                // Wenn als Default markiert, alte Defaults entfernen
                if (layout.IsDefault && !existing.IsDefault)
                {
                    await ClearDefaultFlagsAsync(context);
                }

                existing.Name = layout.Name;
                existing.Description = layout.Description;
                existing.BackgroundSvg = layout.BackgroundSvg;
                existing.Width = layout.Width;
                existing.Height = layout.Height;
                existing.GridSize = layout.GridSize;
                existing.IsDefault = layout.IsDefault;
                existing.IsActive = layout.IsActive;
                existing.SortOrder = layout.SortOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                context.LayoutConfigurations.Update(existing);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Layout '{layout.Name}' (ID: {layout.Id}) aktualisiert");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Aktualisieren von Layout {layout.Id}");
                return false;
            }
        }

        /// <summary>
        /// Löscht ein Layout (und alle zugehörigen Positionen durch Cascade)
        /// </summary>
        public async Task<bool> DeleteLayoutAsync(int layoutId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var layout = await context.LayoutConfigurations
                    .FirstOrDefaultAsync(l => l.Id == layoutId);

                if (layout == null)
                    return false;

                // Standard-Layout kann nicht gelöscht werden
                if (layout.IsDefault)
                {
                    _logger.LogWarning($"Versuch, Standard-Layout (ID: {layoutId}) zu löschen");
                    return false;
                }

                context.LayoutConfigurations.Remove(layout);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Layout '{layout.Name}' (ID: {layoutId}) gelöscht");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Löschen von Layout {layoutId}");
                return false;
            }
        }

        #endregion

        #region Positions-Verwaltung

        /// <summary>
        /// Gibt alle Positionen eines Layouts zurück
        /// </summary>
        public async Task<List<DriveLayoutPosition>> GetLayoutPositionsAsync(int layoutId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveLayoutPositions
                .Where(p => p.LayoutId == layoutId)
                .OrderBy(p => p.ZIndex)
                .ThenBy(p => p.DriveId)
                .ToListAsync();
        }

        /// <summary>
        /// Gibt die Position eines Antriebs in einem Layout zurück
        /// </summary>
        public async Task<DriveLayoutPosition?> GetDrivePositionAsync(int layoutId, int driveId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.DriveLayoutPositions
                .FirstOrDefaultAsync(p => p.LayoutId == layoutId && p.DriveId == driveId);
        }

        /// <summary>
        /// Setzt oder aktualisiert die Position eines Antriebs
        /// </summary>
        public async Task<DriveLayoutPosition> SavePositionAsync(DriveLayoutPosition position)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.DriveLayoutPositions
                .FirstOrDefaultAsync(p => p.LayoutId == position.LayoutId && p.DriveId == position.DriveId);

            if (existing != null)
            {
                // Update
                existing.X = position.X;
                existing.Y = position.Y;
                existing.Width = position.Width;
                existing.Height = position.Height;
                existing.Rotation = position.Rotation;
                existing.ShapeType = position.ShapeType;
                existing.CustomColor = position.CustomColor;
                existing.ShowLabel = position.ShowLabel;
                existing.LabelPosition = position.LabelPosition;
                existing.IsVisible = position.IsVisible;
                existing.ZIndex = position.ZIndex;
                existing.UpdatedAt = DateTime.UtcNow;

                context.DriveLayoutPositions.Update(existing);
                await context.SaveChangesAsync();
                return existing;
            }
            else
            {
                // Insert
                position.CreatedAt = DateTime.UtcNow;
                position.UpdatedAt = DateTime.UtcNow;

                context.DriveLayoutPositions.Add(position);
                await context.SaveChangesAsync();
                return position;
            }
        }

        /// <summary>
        /// Speichert mehrere Positionen gleichzeitig (Bulk Update)
        /// </summary>
        public async Task<bool> SavePositionsAsync(List<DriveLayoutPosition> positions)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                foreach (var position in positions)
                {
                    var existing = await context.DriveLayoutPositions
                        .FirstOrDefaultAsync(p => p.LayoutId == position.LayoutId && p.DriveId == position.DriveId);

                    if (existing != null)
                    {
                        existing.X = position.X;
                        existing.Y = position.Y;
                        existing.Width = position.Width;
                        existing.Height = position.Height;
                        existing.Rotation = position.Rotation;
                        existing.ShapeType = position.ShapeType;
                        existing.CustomColor = position.CustomColor;
                        existing.ShowLabel = position.ShowLabel;
                        existing.LabelPosition = position.LabelPosition;
                        existing.IsVisible = position.IsVisible;
                        existing.ZIndex = position.ZIndex;
                        existing.UpdatedAt = DateTime.UtcNow;

                        context.DriveLayoutPositions.Update(existing);
                    }
                    else
                    {
                        position.CreatedAt = DateTime.UtcNow;
                        position.UpdatedAt = DateTime.UtcNow;
                        context.DriveLayoutPositions.Add(position);
                    }
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern mehrerer Positionen");
                return false;
            }
        }

        /// <summary>
        /// Löscht die Position eines Antriebs aus einem Layout
        /// </summary>
        public async Task<bool> DeletePositionAsync(int layoutId, int driveId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var position = await context.DriveLayoutPositions
                    .FirstOrDefaultAsync(p => p.LayoutId == layoutId && p.DriveId == driveId);

                if (position == null)
                    return false;

                context.DriveLayoutPositions.Remove(position);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Löschen der Position (Layout: {layoutId}, Drive: {driveId})");
                return false;
            }
        }

        /// <summary>
        /// Erstellt Auto-Layout für alle aktiven Antriebe in einem Grid
        /// </summary>
        public async Task<bool> GenerateGridLayoutAsync(int layoutId, int columns = 10, double spacing = 120)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // Hole alle aktiven Antriebe
                var activeConfigs = await context.DriveConfigurations
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Id)
                    .ToListAsync();

                var positions = new List<DriveLayoutPosition>();
                int column = 0;
                int row = 0;

                foreach (var config in activeConfigs)
                {
                    positions.Add(new DriveLayoutPosition
                    {
                        LayoutId = layoutId,
                        DriveId = config.Id,
                        X = 50 + (column * spacing),
                        Y = 50 + (row * spacing),
                        Width = 100,
                        Height = 80,
                        ShapeType = DriveShapeType.Rectangle,
                        ShowLabel = true,
                        LabelPosition = LabelPositionType.Bottom,
                        IsVisible = true,
                        ZIndex = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });

                    column++;
                    if (column >= columns)
                    {
                        column = 0;
                        row++;
                    }
                }

                // Alte Positionen löschen
                var oldPositions = await context.DriveLayoutPositions
                    .Where(p => p.LayoutId == layoutId)
                    .ToListAsync();
                context.DriveLayoutPositions.RemoveRange(oldPositions);

                // Neue Positionen hinzufügen
                context.DriveLayoutPositions.AddRange(positions);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Grid-Layout für Layout {layoutId} generiert ({positions.Count} Antriebe)");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Generieren des Grid-Layouts für Layout {layoutId}");
                return false;
            }
        }

        #endregion

        #region Hilfsmethoden

        private async Task ClearDefaultFlagsAsync(ApplicationDbContext context)
        {
            var defaults = await context.LayoutConfigurations
                .Where(l => l.IsDefault)
                .ToListAsync();

            foreach (var layout in defaults)
            {
                layout.IsDefault = false;
                layout.UpdatedAt = DateTime.UtcNow;
            }

            context.LayoutConfigurations.UpdateRange(defaults);
            await context.SaveChangesAsync();
        }

        #endregion
    }
}
