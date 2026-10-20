using MySqlConnector;
using SchoolPC.Admin.Models;
using System.Diagnostics;

namespace SchoolPC.Admin.Data
{
    public class DatabaseConnectionFactory
    {
        private readonly string _connectionString;

        public DatabaseConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }

    public interface ISessionRepository
    {
        Task<List<Session>> GetSessionsByPcGuidAsync(string clientGuid, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Session?> GetActiveSessionByStudentIdAsync(string studentId);
        Task InsertSessionAsync(Session session);
        Task UpdateSessionLogoutAsync(int sessionId, DateTime logoutTime);
    }

    public class SessionRepository : ISessionRepository
    {
        private readonly DatabaseConnectionFactory _dbFactory;

        public SessionRepository(DatabaseConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Session>> GetSessionsByPcGuidAsync(string clientGuid, DateTime? fromDate = null, DateTime? toDate = null)
        {
            Debug.WriteLine($"[SessionRepository.GetSessionsByPcGuidAsync] Called with GUID: {clientGuid}, FromDate: {fromDate:yyyy-MM-dd}, ToDate: {toDate:yyyy-MM-dd}");
            var sessions = new List<Session>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                // JOIN students for email, select end_reason and recovered_at
                var query = @"
                    SELECT 
                        s.session_id, s.client_guid, s.pc_name, 
                        s.student_id, s.student_name, st.email,
                        s.login_time, s.logout_time, s.duration_minutes,
                        s.end_reason, s.recovered_at
                    FROM sessions s
                    JOIN students st ON st.student_id = s.student_id
                    WHERE s.client_guid = @clientGuid";

                if (fromDate.HasValue && toDate.HasValue)
                {
                    // Strict < on the to_date + 1 day, so whole days are included
                    query += " AND s.login_time >= @fromDate AND s.login_time < DATE_ADD(@toDate, INTERVAL 1 DAY)";
                    Debug.WriteLine($"[SessionRepository.GetSessionsByPcGuidAsync] Date filter added: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
                }

                query += " ORDER BY s.login_time DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", clientGuid);
                    if (fromDate.HasValue)
                        cmd.Parameters.AddWithValue("@fromDate", fromDate.Value);
                    if (toDate.HasValue)
                        cmd.Parameters.AddWithValue("@toDate", toDate.Value);

                    Debug.WriteLine($"[SessionRepository.GetSessionsByPcGuidAsync] Executing query: {query}");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Get ordinal positions for columns
                        int sessionIdOrd = reader.GetOrdinal("session_id");
                        int clientGuidOrd = reader.GetOrdinal("client_guid");
                        int pcNameOrd = reader.GetOrdinal("pc_name");
                        int studentIdOrd = reader.GetOrdinal("student_id");
                        int studentNameOrd = reader.GetOrdinal("student_name");
                        int emailOrd = reader.GetOrdinal("email");
                        int loginTimeOrd = reader.GetOrdinal("login_time");
                        int logoutTimeOrd = reader.GetOrdinal("logout_time");
                        int durationMinutesOrd = reader.GetOrdinal("duration_minutes");
                        int endReasonOrd = reader.GetOrdinal("end_reason");
                        int recoveredAtOrd = reader.GetOrdinal("recovered_at");

                        while (await reader.ReadAsync())
                        {
                            var session = new Session
                            {
                                SessionId = reader.GetInt32(sessionIdOrd),
                                ClientGuid = reader.IsDBNull(clientGuidOrd) ? null : reader.GetString(clientGuidOrd),
                                PcName = reader.GetString(pcNameOrd),
                                StudentId = reader.GetString(studentIdOrd),
                                StudentName = reader.GetString(studentNameOrd),
                                StudentEmail = reader.IsDBNull(emailOrd) ? null : reader.GetString(emailOrd),
                                LoginTime = reader.GetDateTime(loginTimeOrd),
                                LogoutTime = reader.IsDBNull(logoutTimeOrd) ? null : reader.GetDateTime(logoutTimeOrd),
                                DurationMinutes = reader.IsDBNull(durationMinutesOrd) ? null : reader.GetInt32(durationMinutesOrd),
                                EndReason = reader.IsDBNull(endReasonOrd) ? null : reader.GetString(endReasonOrd),
                                RecoveredAt = reader.IsDBNull(recoveredAtOrd) ? null : reader.GetDateTime(recoveredAtOrd)
                            };
                            sessions.Add(session);
                            Debug.WriteLine($"[SessionRepository.GetSessionsByPcGuidAsync] Row: {session.StudentName} at {session.LoginTime:yyyy-MM-dd HH:mm:ss}");
                        }
                    }
                }
            }

            Debug.WriteLine($"[SessionRepository.GetSessionsByPcGuidAsync] Returning {sessions.Count} sessions");
            return sessions;
        }

        public async Task<Session?> GetActiveSessionByStudentIdAsync(string studentId)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = "SELECT session_id, client_guid, pc_name, student_id, student_name, login_time, logout_time, duration_minutes FROM sessions WHERE student_id = @studentId AND logout_time IS NULL ORDER BY login_time DESC LIMIT 1";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int sessionIdOrd = reader.GetOrdinal("session_id");
                        int clientGuidOrd = reader.GetOrdinal("client_guid");
                        int pcNameOrd = reader.GetOrdinal("pc_name");
                        int studentIdOrd = reader.GetOrdinal("student_id");
                        int studentNameOrd = reader.GetOrdinal("student_name");
                        int loginTimeOrd = reader.GetOrdinal("login_time");
                        int logoutTimeOrd = reader.GetOrdinal("logout_time");
                        int durationMinutesOrd = reader.GetOrdinal("duration_minutes");

                        if (await reader.ReadAsync())
                        {
                            return new Session
                            {
                                SessionId = reader.GetInt32(sessionIdOrd),
                                ClientGuid = reader.IsDBNull(clientGuidOrd) ? null : reader.GetString(clientGuidOrd),
                                PcName = reader.GetString(pcNameOrd),
                                StudentId = reader.GetString(studentIdOrd),
                                StudentName = reader.GetString(studentNameOrd),
                                LoginTime = reader.GetDateTime(loginTimeOrd),
                                LogoutTime = reader.IsDBNull(logoutTimeOrd) ? null : reader.GetDateTime(logoutTimeOrd),
                                DurationMinutes = reader.IsDBNull(durationMinutesOrd) ? null : reader.GetInt32(durationMinutesOrd)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task InsertSessionAsync(Session session)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO sessions (client_guid, pc_name, student_id, student_name, login_time)
                            VALUES (@clientGuid, @pcName, @studentId, @studentName, @loginTime)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", session.ClientGuid ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@pcName", session.PcName);
                    cmd.Parameters.AddWithValue("@studentId", session.StudentId);
                    cmd.Parameters.AddWithValue("@studentName", session.StudentName);
                    cmd.Parameters.AddWithValue("@loginTime", session.LoginTime);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateSessionLogoutAsync(int sessionId, DateTime logoutTime)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var durationMinutes = (int)((logoutTime - DateTime.Now).TotalMinutes);

                var query = @"UPDATE sessions SET logout_time = @logoutTime, duration_minutes = @durationMinutes WHERE session_id = @sessionId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@logoutTime", logoutTime);
                    cmd.Parameters.AddWithValue("@durationMinutes", durationMinutes);
                    cmd.Parameters.AddWithValue("@sessionId", sessionId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
