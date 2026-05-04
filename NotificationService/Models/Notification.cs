namespace NotificationService.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string? Message { get; set; }

    public string? Type { get; set; } // Bill / Payment / Alert

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}