using SchoolPC.Admin.Data;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Services
{
    public interface IProblemReportService
    {
        Task<List<ProblemReport>> GetReportsByStatusAsync(string? status = null);
        Task<List<ProblemReport>> GetReportsForPcAsync(string clientGuid);
        Task MarkReportResolvedAsync(int reportId, string adminUsername);
        Task ReportProblemAsync(string studentId, string studentName, string pcName, string message);
    }

    public class ProblemReportService : IProblemReportService
    {
        private readonly IProblemReportRepository _reportRepository;

        public ProblemReportService(IProblemReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<ProblemReport>> GetReportsByStatusAsync(string? status = null)
        {
            return await _reportRepository.GetReportsByStatusAsync(status);
        }

        public async Task<List<ProblemReport>> GetReportsForPcAsync(string clientGuid)
        {
            return await _reportRepository.GetReportsByPcGuidAsync(clientGuid);
        }

        public async Task MarkReportResolvedAsync(int reportId, string adminUsername)
        {
            await _reportRepository.UpdateReportStatusAsync(reportId, "resolved", adminUsername);
        }

        public async Task ReportProblemAsync(string studentId, string studentName, string pcName, string message)
        {
            var report = new ProblemReport
            {
                StudentId = studentId,
                StudentName = studentName,
                PcName = pcName,
                Message = message,
                Status = "open",
                CreatedAt = DateTime.Now
            };

            await _reportRepository.InsertReportAsync(report);
        }
    }
}
