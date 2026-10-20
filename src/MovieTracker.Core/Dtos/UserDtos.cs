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
        [Required, StringLength(50, MinimumLength = 2)] string DisplayName,
        [MaxLength(500), Url] string? AvatarUrl,
        [MaxLength(500)] string? Bio);

    public record ChangePasswordRequest(
        [Required] string CurrentPassword,
        [Required, MinLength(8), MaxLength(100)] string NewPassword);
}
