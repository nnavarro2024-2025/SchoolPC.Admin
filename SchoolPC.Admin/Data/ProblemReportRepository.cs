using MySqlConnector;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Data
{
    public interface IProblemReportRepository
    {
        Task<List<ProblemReport>> GetReportsByStatusAsync(string? status = null);
        Task<List<ProblemReport>> GetReportsByPcGuidAsync(string clientGuid);
        Task InsertReportAsync(ProblemReport report);
        Task UpdateReportStatusAsync(int reportId, string status, string resolvedBy);
    }

    public class ProblemReportRepository : IProblemReportRepository
    {
        private readonly DatabaseConnectionFactory _dbFactory;

        public ProblemReportRepository(DatabaseConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<ProblemReport>> GetReportsByStatusAsync(string? status = null)
        {
            var reports = new List<ProblemReport>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT report_id, client_guid, pc_name, student_id, student_name, message, status, created_at, resolved_at, resolved_by 
                            FROM problem_reports";

                if (!string.IsNullOrEmpty(status))
                    query += " WHERE status = @status";

                query += " ORDER BY created_at DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@status", status);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reports.Add(MapReportFromReader(reader));
                        }
                    }
                }
            }

            return reports;
        }

        public async Task<List<ProblemReport>> GetReportsByPcGuidAsync(string clientGuid)
        {
            var reports = new List<ProblemReport>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT report_id, client_guid, pc_name, student_id, student_name, message, status, created_at, resolved_at, resolved_by 
                            FROM problem_reports 
                            WHERE client_guid = @clientGuid 
                            ORDER BY created_at DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", clientGuid);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reports.Add(MapReportFromReader(reader));
                        }
                    }
                }
            }

            return reports;
        }

        public async Task InsertReportAsync(ProblemReport report)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO problem_reports (client_guid, pc_name, student_id, student_name, message, status, created_at)
                            VALUES (@clientGuid, @pcName, @studentId, @studentName, @message, @status, @createdAt)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", report.ClientGuid ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@pcName", report.PcName);
                    cmd.Parameters.AddWithValue("@studentId", report.StudentId);
                    cmd.Parameters.AddWithValue("@studentName", report.StudentName);
                    cmd.Parameters.AddWithValue("@message", report.Message);
                    cmd.Parameters.AddWithValue("@status", report.Status);
                    cmd.Parameters.AddWithValue("@createdAt", report.CreatedAt);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateReportStatusAsync(int reportId, string status, string resolvedBy)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"UPDATE problem_reports SET status = @status, resolved_at = @resolvedAt, resolved_by = @resolvedBy WHERE report_id = @reportId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@resolvedAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@resolvedBy", resolvedBy);
                    cmd.Parameters.AddWithValue("@reportId", reportId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private ProblemReport MapReportFromReader(MySqlDataReader reader)
        {
            int reportIdOrd = reader.GetOrdinal("report_id");
            int clientGuidOrd = reader.GetOrdinal("client_guid");
            int pcNameOrd = reader.GetOrdinal("pc_name");
            int studentIdOrd = reader.GetOrdinal("student_id");
            int studentNameOrd = reader.GetOrdinal("student_name");
            int messageOrd = reader.GetOrdinal("message");
            int statusOrd = reader.GetOrdinal("status");
            int createdAtOrd = reader.GetOrdinal("created_at");
            int resolvedAtOrd = reader.GetOrdinal("resolved_at");
            int resolvedByOrd = reader.GetOrdinal("resolved_by");

            return new ProblemReport
            {
                ReportId = reader.GetInt32(reportIdOrd),
                ClientGuid = reader.IsDBNull(clientGuidOrd) ? null : reader.GetString(clientGuidOrd),
                PcName = reader.GetString(pcNameOrd),
                StudentId = reader.GetString(studentIdOrd),
                StudentName = reader.GetString(studentNameOrd),
                Message = reader.GetString(messageOrd),
                Status = reader.GetString(statusOrd),
                CreatedAt = reader.GetDateTime(createdAtOrd),
                ResolvedAt = reader.IsDBNull(resolvedAtOrd) ? null : reader.GetDateTime(resolvedAtOrd),
                ResolvedBy = reader.IsDBNull(resolvedByOrd) ? null : reader.GetString(resolvedByOrd)
            };
        }
    }
}
