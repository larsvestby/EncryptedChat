namespace EncryptedChat.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public Guid ChatId { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}