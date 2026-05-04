using NotificationService.Models;
using NotificationService.Repositories;

namespace NotificationService.Services;

public class NotificationService
{
    private readonly NotificationRepository _repo;

    public NotificationService(NotificationRepository repo)
    {
        _repo = repo;
    }

    public void Create(int userId, string message, string type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Type = type
        };

        _repo.Add(notification);
    }

    public List<Notification> GetUserNotifications(int userId)
    {
        return _repo.GetByUser(userId);
    }

    public bool CanAccessNotification(int notificationId, int userId)
    {
        var ownerId = _repo.GetOwnerId(notificationId);
        return ownerId.HasValue && ownerId.Value == userId;
    }

    public string MarkAsRead(int id)
    {
        var n = _repo.GetById(id);

        if (n == null)
            throw new Exception("Notification not found");

        n.IsRead = true;
        _repo.Save();

        return "Marked as read";
    }
}
