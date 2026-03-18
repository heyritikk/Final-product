using InternalBudgetTracker.Data;
using InternalBudgetTracker.Enum;
using InternalBudgetTracker.Models;
using System;
using System.Collections.Generic;

namespace InternalBudgetTracker.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            // DI: AppDbContext is injected from Program.cs (AddDbContext)
            _context = context;
        }

        // CREATE NOTIFICATION
        public void CreateNotification(
            int toUserId,
            NotificationType type,
            string message)
        {
            // Called from other services (ExpenseService, BudgetService, etc.)
            // Creates a DB row in t_Notification for the target user.
            var notification = new Notification
            {
                ToUserId = toUserId,
                Type = type,
                Message = message,
                Status = ReadStatus.Unread,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public List<Notification> GetNotifications(int userId)
        {
            return _context.Notifications
                .Where(n =>
                    n.ToUserId == userId &&
                    n.Status == ReadStatus.Unread)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }


        // UNREAD COUNT (Bell icon)

        public int GetUnreadCount(int userId)
        {
            return _context.Notifications
                .Count(n => n.ToUserId == userId && n.Status == ReadStatus.Unread);
        }


        // MARK AS READ
    
        public bool MarkAsRead(int notificationId, int userId)
        {
            var notification = _context.Notifications
                .FirstOrDefault(n =>
                    n.NotificationId == notificationId &&
                    n.ToUserId == userId);

            if (notification == null)
                return false;

            notification.Status = ReadStatus.Read;
            _context.SaveChanges();
            return true;
        }
    }

}


