using Microsoft.AspNetCore.SignalR;
using SchoolPC.Admin.Services;

namespace SchoolPC.Admin.Hubs
{
    /// <summary>
    /// SignalR Hub for monitoring PC instances and student sessions.
    /// Receives events from SchoolPC.Client and persists them to database.
    /// Uses Context.Items["clientGuid"] to associate connections with PC client_guid.
    /// </summary>
    public class MonitoringHub : Hub
    {
        private readonly ISessionService _sessionService;
        private readonly IProblemReportService _problemReportService;
        private readonly IPcInstanceService _pcInstanceService;

        public MonitoringHub(
            ISessionService sessionService,
            IProblemReportService problemReportService,
            IPcInstanceService pcInstanceService)
        {
            _sessionService = sessionService;
            _problemReportService = problemReportService;
            _pcInstanceService = pcInstanceService;
        }

        /// <summary>
        /// Register or update PC instance. Called by SchoolPC.Client on startup or reconnect.
        /// Stores clientGuid in Context.Items for lifetime of this connection.
        /// </summary>
        /// <param name="clientGuid">Unique identifier for the PC client</param>
        /// <param name="machineName">Computer name (e.g., "CLASSROOM-01")</param>
        /// <param name="ipAddress">IP address of the PC</param>
        public async Task RegisterOrUpdatePc(string clientGuid, string machineName, string ipAddress)
        {
            // Store clientGuid in Context.Items for this connection
            Context.Items["clientGuid"] = clientGuid;

            // Register or update PC instance in database
            await _pcInstanceService.RegisterPcAsync(clientGuid, machineName, ipAddress);

            // Notify admin dashboard that PC is online
            await Clients.All.SendAsync("PcOnline", clientGuid, machineName, ipAddress);
        }

        /// <summary>
        /// Notify that a student session has started.
        /// Called by SchoolPC.Client when a student logs in.
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="studentName">Student name</param>
        public async Task NotifySessionStarted(string studentId, string studentName)
        {
            // Get clientGuid from Context.Items (set when RegisterOrUpdatePc was called)
            var clientGuid = Context.Items.TryGetValue("clientGuid", out var guid) ? guid?.ToString() : null;

            // Insert session record (with nullable clientGuid if missing)
            await _sessionService.StartSessionAsync(clientGuid, "", studentId, studentName);

            // Broadcast to admin dashboard
            await Clients.All.SendAsync("SessionStarted", clientGuid, studentId, studentName);
        }

        /// <summary>
        /// Notify that a student session has ended.
        /// Called by SchoolPC.Client when a student logs out.
        /// </summary>
        /// <param name="studentId">Student ID</param>
        public async Task NotifySessionEnded(string studentId)
        {
            // Broadcast to admin dashboard
            await Clients.All.SendAsync("SessionEnded", studentId);
        }

        /// <summary>
        /// Report a problem from a student PC.
        /// Called by SchoolPC.Client when student clicks "Report Problem".
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="studentName">Student name</param>
        /// <param name="pcName">PC name</param>
        /// <param name="message">Problem description</param>
        public async Task ReportProblem(string studentId, string studentName, string pcName, string message)
        {
            // Insert problem report
            await _problemReportService.ReportProblemAsync(studentId, studentName, pcName, message);

            // Broadcast to admin dashboard
            await Clients.All.SendAsync("ProblemReported", studentId, studentName, pcName, message);
        }

        /// <summary>
        /// Called when a PC client disconnects.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var clientGuid = Context.Items.TryGetValue("clientGuid", out var guid) ? guid?.ToString() : null;
            if (!string.IsNullOrEmpty(clientGuid))
            {
                await Clients.All.SendAsync("PcOffline", clientGuid);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
