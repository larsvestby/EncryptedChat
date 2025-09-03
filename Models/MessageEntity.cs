namespace EncryptedChat.Models
{
    public class MessageEntity
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public UserEntity Sender { get; set; } = null!;
        public Guid ChatId { get; set; }
        public ChatEntity Chat { get; set; } = null!;
        public string EncryptedContent { get; set; } = string.Empty; // RSA-encrypted content
        public string Iv { get; set; } = string.Empty; // Initialization vector for AES
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}