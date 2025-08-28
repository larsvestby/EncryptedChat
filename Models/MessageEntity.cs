namespace EncryptedChat.Models
{
    public class MessageEntity
    {
        public Guid Id { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }

        // Navigation properties
        public ChatEntity Chat { get; set; } = null!;
        public UserEntity Sender { get; set; } = null!;
    }
}