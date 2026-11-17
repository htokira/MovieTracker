using System.ComponentModel;
using MovieTracker.Core.Dtos;

namespace MovieTracker.UI.Models;

public class NotificationItem : INotifyPropertyChanged
{
    private bool _isRead;

    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public Guid? ActorId { get; init; }
    public DateTime CreatedAt { get; init; }

    public bool IsRead
    {
        get => _isRead;
        set
        {
            if (_isRead == value) return;
            _isRead = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRead)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUnread)));
        }
    }

    public bool IsUnread => !_isRead;

    public string TimeText
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt.ToUniversalTime();
            if (diff.TotalMinutes < 1) return "щойно";
            if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes} хв тому";
            if (diff.TotalDays < 1) return $"{(int)diff.TotalHours} год тому";
            return CreatedAt.ToLocalTime().ToString("dd.MM.yyyy");
        }
    }

    public static NotificationItem From(NotificationResponse dto) => new()
    {
        Id = dto.Id,
        Text = dto.Text,
        ActorId = dto.ActorId,
        CreatedAt = dto.CreatedAt,
        IsRead = dto.IsRead
    };

    public event PropertyChangedEventHandler? PropertyChanged;
}
