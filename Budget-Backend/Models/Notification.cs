using InternalBudgetTracker.Enum;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalBudgetTracker.Models
{

    public class Notification
    {
        public int NotificationId { get; set; }

        // user who receive notification
        public int ToUserId { get; set; }
        public User ToUser { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public NotificationType Type { get; set; }

        public string Message { get; set; }
        
        [Column(TypeName = "nvarchar(20)")]
        public ReadStatus Status { get; set; } = ReadStatus.Unread;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
