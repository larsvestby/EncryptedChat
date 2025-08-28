using EncryptedChat.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedChat.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<UserEntity> RegisterUser(string username, string email, string password, string firstName, string lastName)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                throw new Exception("Username or email already exists");

            // Generate salt and hash password
            var salt = GenerateSalt();
            var passwordHash = HashPassword(password, salt);

            // Create user entity
            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = passwordHash,
                Salt = Convert.ToBase64String(salt),
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<UserEntity> ValidateUser(string username, string password)
        {
            // Find user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            // Verify password
            var salt = Convert.FromBase64String(user.Salt);
            var hashedPassword = HashPassword(password, salt);

            return hashedPassword == user.PasswordHash ? user : null;
        }

        private byte[] GenerateSalt()
        {
            var salt = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(32); // 256 bits
                return Convert.ToBase64String(hash);
            }
        }
    }
}