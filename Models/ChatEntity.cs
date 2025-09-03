namespace EncryptedChat.Models
{
    public class ChatEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public UserEntity Owner { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Participants in the chat
        public ICollection<UserEntity> Participants { get; set; } = new List<UserEntity>();

        // Messages in the chat
        public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    }
}