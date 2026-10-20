using MySqlConnector;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Data
{
    public interface IPcInstanceRepository
    {
        Task<List<PcInstance>> GetAllActivePcsAsync();
        Task<List<PcInstance>> GetAllPcsAsync();
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

                var query = @"SELECT instance_id, client_guid, first_pc_name, current_pc_name, display_name, ip_address, 
                                    is_active, first_seen, last_seen, group_id, sort_order,
                                    (TIMESTAMPDIFF(SECOND, last_seen, NOW()) <= 120) AS is_online
                            FROM pc_instances 
                            WHERE is_active = TRUE
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

            System.Diagnostics.Debug.WriteLine($"[PcInstanceRepository] GetAllActivePcsAsync returned {pcs.Count} rows");
            return pcs;
        }

        public async Task<List<PcInstance>> GetAllPcsAsync()
        {
            var pcs = new List<PcInstance>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT instance_id, client_guid, first_pc_name, current_pc_name, display_name, ip_address, 
                                    is_active, first_seen, last_seen, group_id, sort_order,
                                    (TIMESTAMPDIFF(SECOND, last_seen, NOW()) <= 120) AS is_online
                            FROM pc_instances 
                            ORDER BY is_active DESC, last_seen DESC";

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

            System.Diagnostics.Debug.WriteLine($"[PcInstanceRepository] GetAllPcsAsync returned {pcs.Count} rows");
            return pcs;
        }

        public async Task<PcInstance?> GetPcByGuidAsync(string clientGuid)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT instance_id, client_guid, first_pc_name, current_pc_name, display_name, ip_address, 
                                    is_active, first_seen, last_seen, group_id, sort_order,
                                    (TIMESTAMPDIFF(SECOND, last_seen, NOW()) <= 120) AS is_online
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

                var query = @"INSERT INTO pc_instances (client_guid, first_pc_name, current_pc_name, display_name, ip_address, is_active, first_seen, last_seen)
                            VALUES (@clientGuid, @firstPcName, @currentPcName, @displayName, @ipAddress, @isActive, @firstSeen, @lastSeen)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientGuid", pc.ClientGuid);
                    cmd.Parameters.AddWithValue("@firstPcName", pc.FirstPcName);
                    cmd.Parameters.AddWithValue("@currentPcName", pc.CurrentPcName);
                    cmd.Parameters.AddWithValue("@displayName", pc.DisplayName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ipAddress", pc.IpAddress ?? (object)DBNull.Value);
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
                                display_name = @displayName
                            WHERE client_guid = @clientGuid";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@currentPcName", pc.CurrentPcName);
                    cmd.Parameters.AddWithValue("@ipAddress", pc.IpAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@displayName", pc.DisplayName ?? (object)DBNull.Value);
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
            int ipAddressOrd = reader.GetOrdinal("ip_address");
            int isActiveOrd = reader.GetOrdinal("is_active");
            int firstSeenOrd = reader.GetOrdinal("first_seen");
            int lastSeenOrd = reader.GetOrdinal("last_seen");
            int groupIdOrd = reader.GetOrdinal("group_id");
            int sortOrderOrd = reader.GetOrdinal("sort_order");
            int isOnlineOrd = reader.GetOrdinal("is_online");

            return new PcInstance
            {
                InstanceId = reader.GetInt32(instanceIdOrd),
                ClientGuid = reader.GetString(clientGuidOrd),
                FirstPcName = reader.GetString(firstPcNameOrd),
                CurrentPcName = reader.GetString(currentPcNameOrd),
                DisplayName = reader.IsDBNull(displayNameOrd) ? null : reader.GetString(displayNameOrd),
                IpAddress = reader.IsDBNull(ipAddressOrd) ? null : reader.GetString(ipAddressOrd),
                IsActive = reader.GetBoolean(isActiveOrd),
                FirstSeen = reader.GetDateTime(firstSeenOrd),
                LastSeen = reader.GetDateTime(lastSeenOrd),
                GroupId = reader.IsDBNull(groupIdOrd) ? null : reader.GetInt32(groupIdOrd),
                SortOrder = reader.GetInt32(sortOrderOrd),
                IsOnline = reader.GetInt32(isOnlineOrd) != 0  // MySQL TIMESTAMPDIFF comparison returns 0 or 1 as integer
            };
        }
    }
}
