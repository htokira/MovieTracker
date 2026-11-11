using System;
using System.Collections.Generic;
using System.Text;

namespace MovieTracker.Core.Dtos
{
    public record RecommendationsResponse(
        bool IsPersonalized,
        IReadOnlyList<MovieListItemDto> Items);
}
