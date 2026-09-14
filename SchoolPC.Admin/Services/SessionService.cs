using SchoolPC.Admin.Data;
using SchoolPC.Admin.Models;
using System.Text;

namespace SchoolPC.Admin.Services
{
    public interface ISessionService
    {
        Task<List<Session>> GetSessionsForPcAsync(string clientGuid, DateTime? fromDate = null, DateTime? toDate = null);
        Task<string> ExportSessionsToCsvAsync(List<Session> sessions, PcInstance pc);
        Task StartSessionAsync(string? clientGuid, string pcName, string studentId, string studentName);
        Task EndSessionAsync(int sessionId, string studentName);
    }

    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IPcInstanceRepository _pcRepository;

        public SessionService(ISessionRepository sessionRepository, IPcInstanceRepository pcRepository)
        {
            _sessionRepository = sessionRepository;
            _pcRepository = pcRepository;
        }

        public async Task<List<Session>> GetSessionsForPcAsync(string clientGuid, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _sessionRepository.GetSessionsByPcGuidAsync(clientGuid, fromDate, toDate);
        }

        public async Task<string> ExportSessionsToCsvAsync(List<Session> sessions, PcInstance pc)
        {
            var csv = new StringBuilder();
            csv.AppendLine("PC Name,Student Name,Student ID,Email,Time In,Time Out,Duration (minutes)");

            foreach (var session in sessions)
            {
                var timeIn = session.LoginTime.ToString("g");
                var timeOut = session.LogoutTime?.ToString("g") ?? "";
                var duration = session.DurationMinutes?.ToString() ?? "";

                csv.AppendLine($"\"{pc.DisplayName ?? pc.CurrentPcName}\",\"{session.StudentName}\",\"{session.StudentId}\",\"{session.StudentId}@uic.edu.ph\",\"{timeIn}\",\"{timeOut}\",\"{duration}\"");
            }

            return csv.ToString();
        }

        public async Task StartSessionAsync(string? clientGuid, string pcName, string studentId, string studentName)
        {
            var session = new Session
            {
                ClientGuid = clientGuid,
                PcName = pcName,
                StudentId = studentId,
                StudentName = studentName,
                LoginTime = DateTime.Now
            };

            await _sessionRepository.InsertSessionAsync(session);
        }

        public async Task EndSessionAsync(int sessionId, string studentName)
        {
            var logoutTime = DateTime.Now;
            await _sessionRepository.UpdateSessionLogoutAsync(sessionId, logoutTime);
        }
    }
}
