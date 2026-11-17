using FluentAssertions;
using MovieTracker.UI.Models;

namespace MovieTracker.Tests.UI;

public class NotificationItemTests
{
    [Fact]
    public void IsUnread_IsOppositeOfIsRead()
    {
        var item = new NotificationItem { IsRead = false };
        item.IsUnread.Should().BeTrue();

        item.IsRead = true;
        item.IsUnread.Should().BeFalse();
    }

    [Fact]
    public void SettingIsRead_RaisesPropertyChangedForIsReadAndIsUnread()
    {
        var item = new NotificationItem();
        var raised = new List<string?>();
        item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        item.IsRead = true;

        raised.Should().Equal(nameof(NotificationItem.IsRead), nameof(NotificationItem.IsUnread));
    }

    [Fact]
    public void SettingIsRead_ToSameValue_DoesNotRaisePropertyChanged()
    {
        var item = new NotificationItem { IsRead = true };
        var raised = 0;
        item.PropertyChanged += (_, _) => raised++;

        item.IsRead = true;

        raised.Should().Be(0);
    }

    [Fact]
    public void TimeText_ForOldNotification_ReturnsLocalDate()
    {
        var created = new DateTime(2020, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var item = new NotificationItem { CreatedAt = created };

        item.TimeText.Should().Be(created.ToLocalTime().ToString("dd.MM.yyyy"));
    }

    [Fact]
    public void TimeText_ForMinutesAgo_StartsWithMinuteCount()
    {
        var item = new NotificationItem { CreatedAt = DateTime.UtcNow.AddMinutes(-5) };

        item.TimeText.Should().StartWith("5");
    }

    [Fact]
    public void TimeText_ForHoursAgo_StartsWithHourCount()
    {
        var item = new NotificationItem { CreatedAt = DateTime.UtcNow.AddHours(-2) };

        item.TimeText.Should().StartWith("2");
    }

    [Fact]
    public void TimeText_ForJustNow_HasNoDigits()
    {
        var item = new NotificationItem { CreatedAt = DateTime.UtcNow };

        item.TimeText.Should().NotMatchRegex(@"\d");
    }
}
