using MySqlConnector;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Data
{
    public interface IPcGroupRepository
    {
        Task<List<PcGroup>> GetAllGroupsAsync();
    }

    public class PcGroupRepository : IPcGroupRepository
    {
        private readonly DatabaseConnectionFactory _dbFactory;

        public PcGroupRepository(DatabaseConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<PcGroup>> GetAllGroupsAsync()
        {
            var groups = new List<PcGroup>();

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT group_id, group_name, sort_order, created_at
                            FROM pc_groups
                            ORDER BY sort_order ASC, group_name ASC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            groups.Add(MapGroupFromReader(reader));
                        }
                    }
                }
            }

            return groups;
        }

        private PcGroup MapGroupFromReader(MySqlDataReader reader)
        {
            return new PcGroup
            {
                GroupId = reader.GetInt32(reader.GetOrdinal("group_id")),
                GroupName = reader.GetString(reader.GetOrdinal("group_name")),
                SortOrder = reader.GetInt32(reader.GetOrdinal("sort_order")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            };
        }
    }
}
