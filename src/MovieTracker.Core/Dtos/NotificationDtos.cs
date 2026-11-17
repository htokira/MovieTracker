namespace MovieTracker.Core.Dtos
{
    public record NotificationResponse(
        Guid Id,
        string Type,
        string Text,
        Guid? ActorId,
        bool IsRead,
        DateTime CreatedAt);
}
