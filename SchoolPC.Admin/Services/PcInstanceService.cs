using SchoolPC.Admin.Data;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Services
{
    public interface IPcInstanceService
    {
        Task<List<PcInstance>> GetAllActivePcsAsync();
        Task<PcInstance?> GetPcByGuidAsync(string clientGuid);
        Task RegisterPcAsync(string clientGuid, string machineName, string ipAddress);
        Task UpdatePcAsync(PcInstance pc);
    }

    public class PcInstanceService : IPcInstanceService
    {
        private readonly IPcInstanceRepository _pcRepository;

        public PcInstanceService(IPcInstanceRepository pcRepository)
        {
            _pcRepository = pcRepository;
        }

        public async Task<List<PcInstance>> GetAllActivePcsAsync()
        {
            return await _pcRepository.GetAllActivePcsAsync();
        }

        public async Task<PcInstance?> GetPcByGuidAsync(string clientGuid)
        {
            return await _pcRepository.GetPcByGuidAsync(clientGuid);
        }

        public async Task RegisterPcAsync(string clientGuid, string machineName, string ipAddress)
        {
            // Check if PC already exists
            var existing = await _pcRepository.GetPcByGuidAsync(clientGuid);
            if (existing != null)
            {
                // Update existing PC
                existing.CurrentPcName = machineName;
                existing.IpAddress = ipAddress;
                existing.LastSeen = DateTime.Now;
                await _pcRepository.UpdatePcInstanceAsync(existing);
            }
            else
            {
                // Insert new PC - default display_name to machine_name if not set
                var pc = new PcInstance
                {
                    ClientGuid = clientGuid,
                    FirstPcName = machineName,
                    CurrentPcName = machineName,
                    DisplayName = machineName,  // ← DEFAULT: use machine name for display
                    IpAddress = ipAddress,
                    IsActive = true,
                    IsApproved = true,  // ← AUTO-APPROVE: immediate visibility (manual approval removed from scope)
                    LastSeen = DateTime.Now
                };
                await _pcRepository.InsertPcInstanceAsync(pc);
            }
        }

        public async Task UpdatePcAsync(PcInstance pc)
        {
            await _pcRepository.UpdatePcInstanceAsync(pc);
        }
    }
}
