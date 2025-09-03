using EncryptedChat.DTOs;
using EncryptedChat.Models;
using EncryptedChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedChat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatService _chatService;

        public ChatController(ApplicationDbContext context, ChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        // Create a new chat between two users
        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatDto createChatDto)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Check if users exist
                var participant1 = await _context.Users.FindAsync(currentUserId);
                var participant2 = await _context.Users.FindAsync(createChatDto.ParticipantId);

                if (participant1 == null || participant2 == null)
                    return NotFound("One or more users not found");

                // Check if chat already exists between these users
                var existingChat = await _context.Chats
                    .Include(c => c.Participants)
                    .FirstOrDefaultAsync(c => c.Participants.Any(p => p.Id == currentUserId) &&
                                             c.Participants.Any(p => p.Id == createChatDto.ParticipantId));

                if (existingChat != null)
                    return Ok(new { ChatId = existingChat.Id, Message = "Chat already exists" });

                // Create new chat
                var chat = new ChatEntity
                {
                    Id = Guid.NewGuid(),
                    Name = createChatDto.ChatName,
                    OwnerId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                // Add participants
                chat.Participants.Add(participant1);
                chat.Participants.Add(participant2);

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                return Ok(new { ChatId = chat.Id, Message = "Chat created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error creating chat", Error = ex.Message });
            }
        }

        // Get all chats for the current user
        [HttpGet("my-chats")]
        public async Task<IActionResult> GetMyChats()
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var chats = await _context.Chats
                    .Include(c => c.Participants)
                    .Include(c => c.Messages)
                    .Where(c => c.Participants.Any(p => p.Id == currentUserId))
                    .Select(c => new ChatDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        OwnerId = c.OwnerId,
                        CreatedAt = c.CreatedAt,
                        ParticipantIds = c.Participants.Select(p => p.Id).ToList(),
                        LastMessage = c.Messages
                            .OrderByDescending(m => m.SentAt)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving chats", Error = ex.Message });
            }
        }

        // Send an encrypted message to a chat
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto sendMessageDto)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Verify user is a participant in the chat
                var chat = await _context.Chats
                    .Include(c => c.Participants)
                    .FirstOrDefaultAsync(c => c.Id == sendMessageDto.ChatId &&
                                             c.Participants.Any(p => p.Id == currentUserId));

                if (chat == null)
                    return NotFound("Chat not found or user not a participant");

                // Get the recipient (the other participant)
                var recipient = chat.Participants.FirstOrDefault(p => p.Id != currentUserId);
                if (recipient == null)
                    return BadRequest("No recipient found in chat");

                // Create message entity
                var message = new MessageEntity
                {
                    Id = Guid.NewGuid(),
                    SenderId = currentUserId,
                    ChatId = sendMessageDto.ChatId,
                    EncryptedContent = sendMessageDto.EncryptedContent,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new { MessageId = message.Id, Message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error sending message", Error = ex.Message });
            }
        }

        // Get messages for a specific chat
        [HttpGet("messages/{chatId}")]
        public async Task<IActionResult> GetMessages(Guid chatId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Verify user is a participant in the chat
                var isParticipant = await _context.Chats
                    .AnyAsync(c => c.Id == chatId &&
                                  c.Participants.Any(p => p.Id == currentUserId));

                if (!isParticipant)
                    return Unauthorized("User is not a participant in this chat");

                var messages = await _context.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.SentAt)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        SenderUsername = m.Sender.Username,
                        ChatId = m.ChatId,
                        EncryptedContent = m.EncryptedContent,
                        SentAt = m.SentAt
                    })
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving messages", Error = ex.Message });
            }
        }

        // Get a user's public key for encryption
        [HttpGet("public-key/{userId}")]
        public async Task<IActionResult> GetPublicKey(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                return Ok(new { PublicKey = user.PublicKey });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving public key", Error = ex.Message });
            }
        }
    }
}