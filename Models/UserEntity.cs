namespace EncryptedChat.Models
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Chats where this user is a participant
        public ICollection<ChatEntity> Chats { get; set; } = new List<ChatEntity>();

        // Chats owned by this user
        public ICollection<ChatEntity> OwnedChats { get; set; } = new List<ChatEntity>();

        // Messages sent by this user
        public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    }
}