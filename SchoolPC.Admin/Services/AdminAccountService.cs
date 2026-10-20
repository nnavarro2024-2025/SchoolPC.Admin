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
            System.Diagnostics.Debug.WriteLine($"[AUTH] AuthenticateAsync called for: {username}");
            var admin = await _repository.GetAdminByUsernameAsync(username);
            if (admin == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Admin not found in database");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"[AUTH] Admin found: {admin.FullName}, IsActive: {admin.IsActive}");

            if (!admin.IsActive)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Admin is inactive");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"[AUTH] Verifying password...");
            if (!_passwordHasher.VerifyPassword(password, admin.PasswordHash))
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Password verification failed");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"[AUTH] Authentication successful!");
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
                IsActive = true
            };

            await _repository.InsertAdminAsync(admin);
        }
    }
}
