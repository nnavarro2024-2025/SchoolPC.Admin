namespace SchoolPC.Admin.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // Use BCrypt.Net-Next to hash password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            // Use BCrypt.Net-Next to verify password against hash
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
