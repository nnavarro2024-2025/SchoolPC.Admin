using MySqlConnector;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Data
{
    public interface IStudentAccountRepository
    {
        Task<Student?> GetStudentByIdAsync(string studentId);
        Task<Student?> GetStudentByUsernameAsync(string username);
        Task<List<Student>> GetAllActiveStudentsAsync();
        Task InsertStudentAsync(Student student);
        Task<List<BannedStudent>> GetBannedStudentsAsync();
        Task InsertBanAsync(BannedStudent ban);
        Task UnbanStudentAsync(int banId);
    }

    public class StudentAccountRepository : IStudentAccountRepository
    {
        private readonly DatabaseConnectionFactory _dbFactory;

        public StudentAccountRepository(DatabaseConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Student?> GetStudentByIdAsync(string studentId)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT student_id, username, password_hash, full_name, email, is_active, created_at 
                            FROM students 
                            WHERE student_id = @studentId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapStudentFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public async Task<Student?> GetStudentByUsernameAsync(string username)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT student_id, username, password_hash, full_name, email, is_active, created_at 
                            FROM students 
                            WHERE username = @username";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapStudentFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public async Task<List<Student>> GetAllActiveStudentsAsync()
        {
            var students = new List<Student>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT student_id, username, password_hash, full_name, email, is_active, created_at 
                            FROM students 
                            WHERE is_active = TRUE
                            ORDER BY full_name";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            students.Add(MapStudentFromReader(reader));
                        }
                    }
                }
            }

            return students;
        }

        public async Task InsertStudentAsync(Student student)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO students (student_id, username, password_hash, full_name, email, is_active, created_at)
                            VALUES (@studentId, @username, @passwordHash, @fullName, @email, @isActive, @createdAt)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@studentId", student.StudentId);
                    cmd.Parameters.AddWithValue("@username", student.Username);
                    cmd.Parameters.AddWithValue("@passwordHash", student.PasswordHash);
                    cmd.Parameters.AddWithValue("@fullName", student.FullName);
                    cmd.Parameters.AddWithValue("@email", student.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isActive", student.IsActive);
                    cmd.Parameters.AddWithValue("@createdAt", student.CreatedAt);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<BannedStudent>> GetBannedStudentsAsync()
        {
            var banned = new List<BannedStudent>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT ban_id, student_id, reason, banned_by, banned_at, is_active 
                            FROM banned_students 
                            WHERE is_active = TRUE
                            ORDER BY banned_at DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int banIdOrd = reader.GetOrdinal("ban_id");
                        int studentIdOrd = reader.GetOrdinal("student_id");
                        int reasonOrd = reader.GetOrdinal("reason");
                        int bannedByOrd = reader.GetOrdinal("banned_by");
                        int bannedAtOrd = reader.GetOrdinal("banned_at");
                        int isActiveOrd = reader.GetOrdinal("is_active");

                        while (await reader.ReadAsync())
                        {
                            banned.Add(new BannedStudent
                            {
                                BanId = reader.GetInt32(banIdOrd),
                                StudentId = reader.GetString(studentIdOrd),
                                Reason = reader.IsDBNull(reasonOrd) ? null : reader.GetString(reasonOrd),
                                BannedBy = reader.IsDBNull(bannedByOrd) ? null : reader.GetString(bannedByOrd),
                                BannedAt = reader.GetDateTime(bannedAtOrd),
                                IsActive = reader.GetBoolean(isActiveOrd)
                            });
                        }
                    }
                }
            }

            return banned;
        }

        public async Task InsertBanAsync(BannedStudent ban)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO banned_students (student_id, reason, banned_by, banned_at, is_active)
                            VALUES (@studentId, @reason, @bannedBy, @bannedAt, @isActive)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@studentId", ban.StudentId);
                    cmd.Parameters.AddWithValue("@reason", ban.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@bannedBy", ban.BannedBy ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@bannedAt", ban.BannedAt);
                    cmd.Parameters.AddWithValue("@isActive", ban.IsActive);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UnbanStudentAsync(int banId)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"UPDATE banned_students SET is_active = FALSE WHERE ban_id = @banId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@banId", banId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private Student MapStudentFromReader(MySqlDataReader reader)
        {
            int studentIdOrd = reader.GetOrdinal("student_id");
            int usernameOrd = reader.GetOrdinal("username");
            int passwordHashOrd = reader.GetOrdinal("password_hash");
            int fullNameOrd = reader.GetOrdinal("full_name");
            int emailOrd = reader.GetOrdinal("email");
            int isActiveOrd = reader.GetOrdinal("is_active");
            int createdAtOrd = reader.GetOrdinal("created_at");

            return new Student
            {
                StudentId = reader.GetString(studentIdOrd),
                Username = reader.GetString(usernameOrd),
                PasswordHash = reader.GetString(passwordHashOrd),
                FullName = reader.GetString(fullNameOrd),
                Email = reader.IsDBNull(emailOrd) ? null : reader.GetString(emailOrd),
                IsActive = reader.GetBoolean(isActiveOrd),
                CreatedAt = reader.GetDateTime(createdAtOrd)
            };
        }
    }
}
