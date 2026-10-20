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
            if (string.IsNullOrEmpty(hash))
            {
                System.Diagnostics.Debug.WriteLine($"[HASH] Hash is null or empty!");
                return false;
            }

            var result = BCrypt.Net.BCrypt.Verify(password, hash);
            System.Diagnostics.Debug.WriteLine($"[HASH] VerifyPassword result: {result} (hash starts with: {hash.Substring(0, Math.Min(10, hash.Length))})");
            return result;
        }
    }
}
