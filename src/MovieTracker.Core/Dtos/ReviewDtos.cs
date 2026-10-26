using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MovieTracker.Core.Dtos
{
    public record UpsertReviewRequest(
        [Range(1, 10)] int Rating,
        [MaxLength(2000)] string? Text);

    public record ReviewResponse(
        Guid Id,
        int MovieId,
        Guid UserId,
        string AuthorName,
        int Rating,
        string? Text,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public record MovieReviewsResponse(
        int MovieId,
        double? AverageRating,
        int TotalReviews,
        int Page,
        int PageSize,
        IReadOnlyList<ReviewResponse> Items);
}
