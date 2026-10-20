using MySqlConnector;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Data
{
    public interface IAdminAccountRepository
    {
        Task<AdminAccount?> GetAdminByUsernameAsync(string username);
        Task<List<AdminAccount>> GetAllAdminsAsync();
        Task InsertAdminAsync(AdminAccount admin);
    }

    public class AdminAccountRepository : IAdminAccountRepository
    {
        private readonly DatabaseConnectionFactory _dbFactory;

        public AdminAccountRepository(DatabaseConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<AdminAccount?> GetAdminByUsernameAsync(string username)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT admin_id, username, password_hash, full_name, role, is_active, created_at 
                            FROM admin_accounts 
                            WHERE username = @username";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var admin = MapAdminFromReader(reader);
                            System.Diagnostics.Debug.WriteLine($"[DB] Found admin: {username}, PasswordHash length: {admin.PasswordHash?.Length ?? 0}");
                            return admin;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[DB] Admin not found: {username}");
                        }
                    }
                }
            }

            return null;
        }

        public async Task<List<AdminAccount>> GetAllAdminsAsync()
        {
            var admins = new List<AdminAccount>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT admin_id, username, password_hash, full_name, role, is_active, created_at 
                            FROM admin_accounts 
                            WHERE is_active = TRUE
                            ORDER BY full_name";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            admins.Add(MapAdminFromReader(reader));
                        }
                    }
                }
            }

            return admins;
        }

        public async Task InsertAdminAsync(AdminAccount admin)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO admin_accounts (username, password_hash, full_name, role, is_active)
                            VALUES (@username, @passwordHash, @fullName, @role, @isActive)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", admin.Username);
                    cmd.Parameters.AddWithValue("@passwordHash", admin.PasswordHash);
                    cmd.Parameters.AddWithValue("@fullName", admin.FullName);
                    cmd.Parameters.AddWithValue("@role", admin.Role);
                    cmd.Parameters.AddWithValue("@isActive", admin.IsActive);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private AdminAccount MapAdminFromReader(MySqlDataReader reader)
        {
            int adminIdOrd = reader.GetOrdinal("admin_id");
            int usernameOrd = reader.GetOrdinal("username");
            int passwordHashOrd = reader.GetOrdinal("password_hash");
            int fullNameOrd = reader.GetOrdinal("full_name");
            int roleOrd = reader.GetOrdinal("role");
            int isActiveOrd = reader.GetOrdinal("is_active");
            int createdAtOrd = reader.GetOrdinal("created_at");

            return new AdminAccount
            {
                AdminId = reader.GetInt32(adminIdOrd),
                Username = reader.GetString(usernameOrd),
                PasswordHash = reader.IsDBNull(passwordHashOrd) ? string.Empty : reader.GetString(passwordHashOrd),
                FullName = reader.GetString(fullNameOrd),
                Role = reader.GetString(roleOrd),
                IsActive = reader.GetBoolean(isActiveOrd),
                CreatedAt = reader.GetDateTime(createdAtOrd)
            };
        }
    }
}
