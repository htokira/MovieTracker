using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MovieTracker.Core.Dtos
{
    public record UserProfileResponse(
        Guid Id,
        string Email,
        string DisplayName,
        string? AvatarUrl,
        string? Bio,
        DateTime CreatedAt);

    public record UpdateProfileRequest(
        [property: Required, StringLength(50, MinimumLength = 2)] string DisplayName,
        [property: MaxLength(500), Url] string? AvatarUrl,
        [property: MaxLength(500)] string? Bio);

    public record ChangePasswordRequest(
        [property: Required] string CurrentPassword,
        [property: Required, MinLength(8), MaxLength(100)] string NewPassword);
}
