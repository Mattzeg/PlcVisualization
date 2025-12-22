using Microsoft.AspNetCore.SignalR;
using PlcVisualization.Models;
using PlcVisualization.Services;
using System.Threading.Tasks;

namespace PlcVisualization.Hubs
{
    /// <summary>
    /// SignalR Hub für Real-time Kommunikation zwischen Server und Clients
    /// Empfängt Kommandos vom Frontend und versendet PLC-Updates
    /// </summary>
    public class DriveHub : Hub
    {
        private readonly PlcService _plcService;
        private readonly ILogger<DriveHub> _logger;

        public DriveHub(PlcService plcService, ILogger<DriveHub> logger)
        {
            _plcService = plcService;
            _logger = logger;
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Client sich verbindet
        /// Sendet initial alle aktuellen Antriebsdaten
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client verbunden: {Context.ConnectionId}");

            // Sende alle aktuellen Antriebsdaten an den neuen Client
            var allDrives = _plcService.GetAllDriveStates().Values;
            await Clients.Caller.SendAsync("InitialDrives", allDrives);

            // Sende Verbindungsstatus
            await Clients.Caller.SendAsync("PlcConnectionStatus", _plcService.IsConnected);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Client sich trennt
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client getrennt: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Empfängt ein Steuerkommando vom Client und leitet es an die SPS weiter
        /// </summary>
        public async Task SendDriveCommand(DriveCommand command)
        {
            _logger.LogInformation($"Befehl empfangen von {Context.ConnectionId} für Antrieb {command.DriveId}");

            var success = await _plcService.SendCommandAsync(command);

            // Sende Rückmeldung an den aufrufenden Client
            await Clients.Caller.SendAsync("CommandResult", new
            {
                driveId = command.DriveId,
                success = success,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Client fragt nach aktuellem PLC-Verbindungsstatus
        /// </summary>
        public async Task RequestPlcStatus()
        {
            await Clients.Caller.SendAsync("PlcConnectionStatus", _plcService.IsConnected);
        }
    }
}
