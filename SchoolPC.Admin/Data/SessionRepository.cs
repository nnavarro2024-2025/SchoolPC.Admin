using MySqlConnector;
using SchoolPC.Admin.Models;

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
            var sessions = new List<Session>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = "SELECT session_id, client_guid, pc_name, student_id, student_name, login_time, logout_time, duration_minutes FROM sessions WHERE client_guid = @clientGuid";

                if (fromDate.HasValue && toDate.HasValue)
                {
                    query += " AND login_time >= @fromDate AND login_time <= DATE_ADD(@toDate, INTERVAL 1 DAY)";
                }

                query += " ORDER BY login_time DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", clientGuid);
                    if (fromDate.HasValue)
                        cmd.Parameters.AddWithValue("@fromDate", fromDate.Value);
                    if (toDate.HasValue)
                        cmd.Parameters.AddWithValue("@toDate", toDate.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Get ordinal positions for columns
                        int sessionIdOrd = reader.GetOrdinal("session_id");
                        int clientGuidOrd = reader.GetOrdinal("client_guid");
                        int pcNameOrd = reader.GetOrdinal("pc_name");
                        int studentIdOrd = reader.GetOrdinal("student_id");
                        int studentNameOrd = reader.GetOrdinal("student_name");
                        int loginTimeOrd = reader.GetOrdinal("login_time");
                        int logoutTimeOrd = reader.GetOrdinal("logout_time");
                        int durationMinutesOrd = reader.GetOrdinal("duration_minutes");

                        while (await reader.ReadAsync())
                        {
                            sessions.Add(new Session
                            {
                                SessionId = reader.GetInt32(sessionIdOrd),
                                ClientGuid = reader.IsDBNull(clientGuidOrd) ? null : reader.GetString(clientGuidOrd),
                                PcName = reader.GetString(pcNameOrd),
                                StudentId = reader.GetString(studentIdOrd),
                                StudentName = reader.GetString(studentNameOrd),
                                LoginTime = reader.GetDateTime(loginTimeOrd),
                                LogoutTime = reader.IsDBNull(logoutTimeOrd) ? null : reader.GetDateTime(logoutTimeOrd),
                                DurationMinutes = reader.IsDBNull(durationMinutesOrd) ? null : reader.GetInt32(durationMinutesOrd)
                            });
                        }
                    }
                }
            }

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
