using System;

namespace BlazorNotificationTemplate.Shared
{
    public class NotifiMessage
    {
        public string Title { get; set; }
        public DateTime? Time { get; set; } = DateTime.Now;
        public NotifiType Type { get; set; } = NotifiType.none;
        public bool IsPrivate { get; set; } = true;
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
    }

    public enum NotifiType
    {
        none,
        Debug,
        Info,
        Warning,
        Error
    }
}
