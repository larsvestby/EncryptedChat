using System.Security.Cryptography;
using System.Text;

namespace EncryptedChat.Services
{
    public class ChatService
    {
        public string EncryptMessage(string message, string recipientPublicKey)
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(recipientPublicKey);

            // Convert message to bytes
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // Encrypt using RSA
            var encryptedBytes = rsa.Encrypt(messageBytes, false);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string DecryptMessage(string encryptedMessage, string privateKey)
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(privateKey);

            // Convert from base64
            var encryptedBytes = Convert.FromBase64String(encryptedMessage);

            // Decrypt using RSA
            var decryptedBytes = rsa.Decrypt(encryptedBytes, false);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
