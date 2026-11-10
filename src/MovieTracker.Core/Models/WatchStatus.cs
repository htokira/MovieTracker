using System.Text.Json.Serialization;

namespace MovieTracker.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WatchStatus
{
    Watched = 1,
    Planned = 2,
    Dropped = 3
}