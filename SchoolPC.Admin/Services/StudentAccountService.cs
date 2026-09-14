using SchoolPC.Admin.Data;
using SchoolPC.Admin.Models;

namespace SchoolPC.Admin.Services
{
    public interface IStudentAccountService
    {
        Task<Student?> GetStudentByIdAsync(string studentId);
        Task AddStudentAsync(string firstName, string middleName, string lastName, string email, string password);
        Task BanStudentAsync(string studentId, string reason, string bannedBy);
        Task<List<BannedStudent>> GetBannedStudentsAsync();
        Task UnbanStudentAsync(int banId);
    }

    public class StudentAccountService : IStudentAccountService
    {
        private readonly IStudentAccountRepository _repository;
        private readonly IPasswordHasher _passwordHasher;

        public StudentAccountService(IStudentAccountRepository repository, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Student?> GetStudentByIdAsync(string studentId)
        {
            return await _repository.GetStudentByIdAsync(studentId);
        }

        public async Task AddStudentAsync(string firstName, string middleName, string lastName, string email, string password)
        {
            if (!email.EndsWith("@uic.edu.ph"))
                throw new ArgumentException("Email must end with @uic.edu.ph");

            var studentId = ExtractStudentIdFromEmail(email);
            var fullName = $"{firstName.ToUpper()} {(middleName.ToUpper())} {lastName.ToUpper()}".Trim();
            var passwordHash = _passwordHasher.HashPassword(password);

            var student = new Student
            {
                StudentId = studentId,
                Username = studentId,
                PasswordHash = passwordHash,
                FullName = fullName,
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _repository.InsertStudentAsync(student);
        }

        public async Task BanStudentAsync(string studentId, string reason, string bannedBy)
        {
            var ban = new BannedStudent
            {
                StudentId = studentId,
                Reason = reason,
                BannedBy = bannedBy,
                BannedAt = DateTime.Now,
                IsActive = true
            };

            await _repository.InsertBanAsync(ban);
        }

        public async Task<List<BannedStudent>> GetBannedStudentsAsync()
        {
            return await _repository.GetBannedStudentsAsync();
        }

        public async Task UnbanStudentAsync(int banId)
        {
            await _repository.UnbanStudentAsync(banId);
        }

        private string ExtractStudentIdFromEmail(string email)
        {
            var parts = email.Split('_');
            if (parts.Length >= 2)
            {
                var id = parts[1].Replace("@uic.edu.ph", "");
                return id;
            }

            throw new ArgumentException("Could not extract student ID from email");
        }
    }
}
