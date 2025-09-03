using EncryptedChat.Models;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string EncryptedPrivateKey { get; set; } = string.Empty; // Add this
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatEntity> Chats { get; set; } = [];
    public ICollection<ChatEntity> OwnedChats { get; set; } = [];
    public ICollection<MessageEntity> Messages { get; set; } = [];
}