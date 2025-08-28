namespace EncryptedChat.DTOs
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Owner information
        public Guid OwnerId { get; set; }
        public UserDto Owner { get; set; } = null!;

        // Participants (excluding owner)
        public List<UserDto> Participants { get; set; } = new List<UserDto>();

        // Recent messages (you might want to paginate these)
        public List<MessageDto> RecentMessages { get; set; } = new List<MessageDto>();
    }
}