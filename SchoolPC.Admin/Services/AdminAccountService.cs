using SchoolPC.Admin.Data;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Services
{
    public interface IAdminAccountService
    {
        Task<AdminAccount?> AuthenticateAsync(string username, string password);
        Task AddAdminAsync(string username, string password, string fullName, string role = "admin");
    }

    public class AdminAccountService : IAdminAccountService
    {
        private readonly IAdminAccountRepository _repository;
        private readonly IPasswordHasher _passwordHasher;

        public AdminAccountService(IAdminAccountRepository repository, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<AdminAccount?> AuthenticateAsync(string username, string password)
        {
            var admin = await _repository.GetAdminByUsernameAsync(username);
            if (admin == null)
                return null;

            if (!admin.IsActive)
                return null;

            if (!_passwordHasher.VerifyPassword(password, admin.PasswordHash))
                return null;

            return admin;
        }

        public async Task AddAdminAsync(string username, string password, string fullName, string role = "admin")
        {
            var passwordHash = _passwordHasher.HashPassword(password);

            var admin = new AdminAccount
            {
                Username = username,
                PasswordHash = passwordHash,
                FullName = fullName,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _repository.InsertAdminAsync(admin);
        }
    }
}
