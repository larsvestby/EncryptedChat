namespace EncryptedChat.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }
        public UserDto Sender { get; set; } = null!;
    }
}