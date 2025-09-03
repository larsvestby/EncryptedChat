using EncryptedChat.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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

        // Add this method to AuthService class
        public async Task<UserEntity> GetUserById(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<(UserEntity user, string privateKey)> RegisterUser(string username, string email, string password, string firstName, string lastName)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                throw new Exception("Username or email already exists");

            // Generate salt and hash password
            var salt = GenerateSalt();
            var passwordHash = HashPassword(password, salt);

            // Generate RSA key pair
            var (publicKey, encryptedPrivateKey) = GenerateRsaKeyPair(password, salt);

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
                PublicKey = publicKey,
                EncryptedPrivateKey = encryptedPrivateKey, // Add this property to UserEntity
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (user, encryptedPrivateKey);
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

        private (string publicKey, string encryptedPrivateKey) GenerateRsaKeyPair(string password, byte[] salt)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    // Generate key pair
                    var publicKey = rsa.ToXmlString(false);
                    var privateKey = rsa.ToXmlString(true);

                    // Encrypt the private key with the user's password
                    var encryptedPrivateKey = EncryptPrivateKey(privateKey, password, salt);

                    return (publicKey, encryptedPrivateKey);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
        private string EncryptPrivateKey(string privateKey, string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                var key = deriveBytes.GetBytes(32); // 256-bit key
                var iv = deriveBytes.GetBytes(16);  // 128-bit IV

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(privateKey);
                        }

                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public string DecryptPrivateKey(string encryptedPrivateKey, string password, string saltBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                var key = deriveBytes.GetBytes(32); // 256-bit key
                var iv = deriveBytes.GetBytes(16);  // 128-bit IV

                var encryptedData = Convert.FromBase64String(encryptedPrivateKey);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(encryptedData))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}