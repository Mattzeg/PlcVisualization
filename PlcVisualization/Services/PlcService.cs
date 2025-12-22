using S7.Net;
using PlcVisualization.Models;
using PlcVisualization.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlcVisualization.Services
{
    /// <summary>
    /// Background Service für die Kommunikation mit der Siemens S7-1500 SPS
    /// Liest zyklisch alle Antriebsdaten und sendet Updates via SignalR
    /// </summary>
    public class PlcService : BackgroundService
    {
        private readonly ILogger<PlcService> _logger;
        private readonly IHubContext<DriveHub> _hubContext;
        private readonly PlcSettings _settings;
        private Plc? _plc;
        private readonly Dictionary<int, DriveState> _driveStates = new();
        private bool _isConnected = false;

        public PlcService(
            ILogger<PlcService> logger,
            IHubContext<DriveHub> hubContext,
            IOptions<PlcSettings> settings)
        {
            _logger = logger;
            _hubContext = hubContext;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PLC Service wird gestartet...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_isConnected)
                    {
                        await ConnectToPlcAsync();
                    }

                    if (_isConnected)
                    {
                        await ReadAllDrivesAsync();
                        await Task.Delay(_settings.UpdateIntervalMs, stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("Verbindung zur PLC nicht möglich. Retry in 5 Sekunden...");
                        await Task.Delay(5000, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Service wird gestoppt - normal
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler im PLC Service");
                    _isConnected = false;
                    _plc?.Close();
                    await Task.Delay(5000, stoppingToken);
                }
            }

            _logger.LogInformation("PLC Service wurde gestoppt");
        }

        /// <summary>
        /// Verbindung zur SPS herstellen
        /// </summary>
        private async Task ConnectToPlcAsync()
        {
            try
            {
                _logger.LogInformation($"Verbinde mit PLC auf {_settings.IpAddress}...");

                _plc = new Plc(CpuType.S71500, _settings.IpAddress, (short)_settings.Rack, (short)_settings.Slot);
                await _plc.OpenAsync();

                _isConnected = _plc.IsConnected;

                if (_isConnected)
                {
                    _logger.LogInformation($"✓ Erfolgreich verbunden mit PLC: {_settings.IpAddress}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Verbindung zur PLC {_settings.IpAddress} fehlgeschlagen");
                _isConnected = false;
            }
        }

        /// <summary>
        /// Liest alle 100 Antriebe aus der SPS (optimiert in einem Rutsch)
        /// </summary>
        private async Task ReadAllDrivesAsync()
        {
            if (_plc == null || !_isConnected) return;

            try
            {
                // Alle Antriebe in einem Rutsch lesen (10 Bytes pro Antrieb)
                var drives = new DriveData[_settings.DriveCount];

                // ReadClass liest das komplette Array aus dem konfigurierten DB
                _plc.ReadClass(drives, _settings.DataBlock, 0);

                // States aktualisieren und nur geänderte Antriebe an Frontend senden
                var updatedDrives = new List<DriveState>();

                for (int i = 0; i < _settings.DriveCount; i++)
                {
                    var driveData = drives[i];
                    var driveId = i + 1;

                    // Prüfen ob sich etwas geändert hat (um unnötige SignalR-Updates zu vermeiden)
                    if (!_driveStates.ContainsKey(driveId) ||
                        HasChanged(_driveStates[driveId], driveData))
                    {
                        var state = new DriveState
                        {
                            Id = driveId,
                            Name = $"Antrieb {driveId}",
                            ModeAuto = driveData.ModeAuto,
                            Running = driveData.Running,
                            Forward = driveData.Forward,
                            Reverse = driveData.Reverse,
                            Error = driveData.Error,
                            Speed = driveData.Speed,
                            Current = driveData.Current,
                            Setpoint = driveData.Setpoint,
                            ErrorCode = driveData.ErrorCode,
                            LastUpdate = DateTime.UtcNow,
                            Capabilities = new DriveCapabilities
                            {
                                HasSetpoint = driveData.HasSetpoint,
                                HasSpeedDisplay = driveData.HasSpeed,
                                HasCurrentDisplay = driveData.HasCurrent,
                                HasForward = true,  // TODO: Aus Config-DB laden
                                HasReverse = true,
                                HasErrorDisplay = true
                            }
                        };

                        _driveStates[driveId] = state;
                        updatedDrives.Add(state);
                    }
                }

                // Nur geänderte Drives an alle verbundenen Clients pushen
                if (updatedDrives.Count > 0)
                {
                    await _hubContext.Clients.All.SendAsync("DrivesUpdated", updatedDrives);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Lesen der Antriebe");
                _isConnected = false;
                throw;
            }
        }

        /// <summary>
        /// Prüft ob sich Antriebsdaten geändert haben
        /// </summary>
        private bool HasChanged(DriveState oldState, DriveData newData)
        {
            return oldState.ModeAuto != newData.ModeAuto ||
                   oldState.Running != newData.Running ||
                   oldState.Forward != newData.Forward ||
                   oldState.Reverse != newData.Reverse ||
                   oldState.Error != newData.Error ||
                   oldState.Speed != newData.Speed ||
                   oldState.Current != newData.Current ||
                   oldState.Setpoint != newData.Setpoint ||
                   oldState.ErrorCode != newData.ErrorCode;
        }

        /// <summary>
        /// Sendet ein Kommando an die SPS
        /// </summary>
        public async Task<bool> SendCommandAsync(DriveCommand command)
        {
            if (_plc == null || !_isConnected)
            {
                _logger.LogWarning($"Kann Befehl nicht senden - PLC nicht verbunden");
                return false;
            }

            try
            {
                int driveIndex = command.DriveId - 1;
                int baseOffset = driveIndex * 10; // 10 Bytes pro Antrieb

                // Start/Stop (Running-Flag)
                if (command.Start.HasValue)
                {
                    _plc.WriteBit(DataType.DataBlock, _settings.DataBlock, baseOffset, 1, command.Start.Value);
                    _logger.LogInformation($"Antrieb {command.DriveId}: Start = {command.Start.Value}");
                }

                if (command.Stop.HasValue && command.Stop.Value)
                {
                    _plc.WriteBit(DataType.DataBlock, _settings.DataBlock, baseOffset, 1, false);
                    _logger.LogInformation($"Antrieb {command.DriveId}: Stop");
                }

                // Forward/Reverse
                if (command.Forward.HasValue)
                {
                    _plc.WriteBit(DataType.DataBlock, _settings.DataBlock, baseOffset, 2, command.Forward.Value);
                    _logger.LogInformation($"Antrieb {command.DriveId}: Forward = {command.Forward.Value}");
                }

                if (command.Reverse.HasValue)
                {
                    _plc.WriteBit(DataType.DataBlock, _settings.DataBlock, baseOffset, 3, command.Reverse.Value);
                    _logger.LogInformation($"Antrieb {command.DriveId}: Reverse = {command.Reverse.Value}");
                }

                // Setpoint
                if (command.Setpoint.HasValue)
                {
                    _plc.WriteBytes(DataType.DataBlock, _settings.DataBlock, baseOffset + 6, S7.Net.Types.Int.ToByteArray(command.Setpoint.Value));
                    _logger.LogInformation($"Antrieb {command.DriveId}: Setpoint = {command.Setpoint.Value}");
                }

                // Mode Auto/Hand
                if (command.ModeAuto.HasValue)
                {
                    _plc.WriteBit(DataType.DataBlock, _settings.DataBlock, baseOffset, 0, command.ModeAuto.Value);
                    _logger.LogInformation($"Antrieb {command.DriveId}: Mode = {(command.ModeAuto.Value ? "AUTO" : "HAND")}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Senden von Befehl an Antrieb {command.DriveId}");
                return false;
            }
        }

        /// <summary>
        /// Gibt alle aktuellen Antriebszustände zurück
        /// </summary>
        public IReadOnlyDictionary<int, DriveState> GetAllDriveStates()
        {
            return _driveStates;
        }

        /// <summary>
        /// Gibt den Verbindungsstatus zurück
        /// </summary>
        public bool IsConnected => _isConnected;

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PLC Service wird gestoppt...");
            _plc?.Close();
            await base.StopAsync(cancellationToken);
        }
    }
}
