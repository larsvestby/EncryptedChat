using EncryptedChat.Models;

namespace EncryptedChat.DTOs
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new List<Guid>();
        public MessageEntity LastMessage { get; set; } = null!;
    }

    public class CreateChatDto
    {
        public Guid ParticipantId { get; set; }
        public string ChatName { get; set; } = string.Empty;
    }

    public class SendMessageDto
    {
        public Guid ChatId { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
    }
}