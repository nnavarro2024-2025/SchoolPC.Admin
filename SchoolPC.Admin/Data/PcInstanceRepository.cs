using MySqlConnector;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Data
{
    public interface IPcInstanceRepository
    {
        Task<List<PcInstance>> GetAllActivePcsAsync();
        Task<PcInstance?> GetPcByGuidAsync(string clientGuid);
        Task InsertPcInstanceAsync(PcInstance pc);
        Task UpdatePcInstanceAsync(PcInstance pc);
        Task<bool> PcExistsByGuidAsync(string clientGuid);
    }

    public class PcInstanceRepository : IPcInstanceRepository
    {
        private readonly DatabaseConnectionFactory _dbFactory;

        public PcInstanceRepository(DatabaseConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<PcInstance>> GetAllActivePcsAsync()
        {
            var pcs = new List<PcInstance>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT instance_id, client_guid, first_pc_name, current_pc_name, display_name, location, ip_address, 
                                    is_approved, is_active, first_seen, last_seen, notes
                            FROM pc_instances 
                            WHERE is_active = TRUE AND is_approved = TRUE
                            ORDER BY last_seen DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pcs.Add(MapPcFromReader(reader));
                        }
                    }
                }
            }

            return pcs;
        }

        public async Task<PcInstance?> GetPcByGuidAsync(string clientGuid)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT instance_id, client_guid, first_pc_name, current_pc_name, display_name, location, ip_address, 
                                    is_approved, is_active, first_seen, last_seen, notes
                            FROM pc_instances 
                            WHERE client_guid = @clientGuid";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", clientGuid);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapPcFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public async Task InsertPcInstanceAsync(PcInstance pc)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO pc_instances (client_guid, first_pc_name, current_pc_name, display_name, location, ip_address, is_approved, is_active, first_seen, last_seen)
                            VALUES (@clientGuid, @firstPcName, @currentPcName, @displayName, @location, @ipAddress, @isApproved, @isActive, @firstSeen, @lastSeen)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", pc.ClientGuid);
                    cmd.Parameters.AddWithValue("@firstPcName", pc.FirstPcName);
                    cmd.Parameters.AddWithValue("@currentPcName", pc.CurrentPcName);
                    cmd.Parameters.AddWithValue("@displayName", pc.DisplayName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@location", pc.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ipAddress", pc.IpAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isApproved", pc.IsApproved);
                    cmd.Parameters.AddWithValue("@isActive", pc.IsActive);
                    cmd.Parameters.AddWithValue("@firstSeen", pc.FirstSeen);
                    cmd.Parameters.AddWithValue("@lastSeen", pc.LastSeen);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdatePcInstanceAsync(PcInstance pc)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"UPDATE pc_instances 
                            SET current_pc_name = @currentPcName, 
                                ip_address = @ipAddress, 
                                last_seen = @lastSeen,
                                display_name = @displayName,
                                location = @location
                            WHERE client_guid = @clientGuid";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@currentPcName", pc.CurrentPcName);
                    cmd.Parameters.AddWithValue("@ipAddress", pc.IpAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@lastSeen", pc.LastSeen);
                    cmd.Parameters.AddWithValue("@displayName", pc.DisplayName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@location", pc.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@clientGuid", pc.ClientGuid);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> PcExistsByGuidAsync(string clientGuid)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = "SELECT COUNT(*) FROM pc_instances WHERE client_guid = @clientGuid";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", clientGuid);
                    var result = await cmd.ExecuteScalarAsync();
                    return (long?)result > 0;
                }
            }
        }

        private PcInstance MapPcFromReader(MySqlDataReader reader)
        {
            int instanceIdOrd = reader.GetOrdinal("instance_id");
            int clientGuidOrd = reader.GetOrdinal("client_guid");
            int firstPcNameOrd = reader.GetOrdinal("first_pc_name");
            int currentPcNameOrd = reader.GetOrdinal("current_pc_name");
            int displayNameOrd = reader.GetOrdinal("display_name");
            int locationOrd = reader.GetOrdinal("location");
            int ipAddressOrd = reader.GetOrdinal("ip_address");
            int isApprovedOrd = reader.GetOrdinal("is_approved");
            int isActiveOrd = reader.GetOrdinal("is_active");
            int firstSeenOrd = reader.GetOrdinal("first_seen");
            int lastSeenOrd = reader.GetOrdinal("last_seen");
            int notesOrd = reader.GetOrdinal("notes");

            return new PcInstance
            {
                InstanceId = reader.GetInt32(instanceIdOrd),
                ClientGuid = reader.GetString(clientGuidOrd),
                FirstPcName = reader.GetString(firstPcNameOrd),
                CurrentPcName = reader.GetString(currentPcNameOrd),
                DisplayName = reader.IsDBNull(displayNameOrd) ? null : reader.GetString(displayNameOrd),
                Location = reader.IsDBNull(locationOrd) ? null : reader.GetString(locationOrd),
                IpAddress = reader.IsDBNull(ipAddressOrd) ? null : reader.GetString(ipAddressOrd),
                IsApproved = reader.GetBoolean(isApprovedOrd),
                IsActive = reader.GetBoolean(isActiveOrd),
                FirstSeen = reader.GetDateTime(firstSeenOrd),
                LastSeen = reader.GetDateTime(lastSeenOrd),
                Notes = reader.IsDBNull(notesOrd) ? null : reader.GetString(notesOrd)
            };
        }
    }
}
