namespace MovieTracker.Core.Models
{
    public enum NotificationType
    {
        NewFollower = 1
    }

    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Кому адресоване сповіщення
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public NotificationType Type { get; set; }

        // Хто спричинив подію (може бути порожнім)
        public Guid? ActorId { get; set; }

        public string Text { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
