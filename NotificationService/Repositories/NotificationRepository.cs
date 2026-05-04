using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Repositories;

public class NotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Notification notification)
    {
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public List<Notification> GetByUser(int userId)
    {
        return _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    public Notification? GetById(int id)
    {
        return _context.Notifications.Find(id);
    }

    public int? GetOwnerId(int id)
    {
        return _context.Notifications
            .Where(n => n.Id == id)
            .Select(n => (int?)n.UserId)
            .FirstOrDefault();
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
