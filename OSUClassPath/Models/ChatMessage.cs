using System.ComponentModel.DataAnnotations;

namespace OSUClassPath.Models;

public class ChatMessage
{
    public int Id { get; set; }

    public int ChatSessionId { get; set; }

    public ChatSession? ChatSession { get; set; }

    [StringLength(20)]
    public string Role { get; set; } = "user";

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
