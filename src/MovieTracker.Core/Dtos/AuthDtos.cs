using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MovieTracker.Core.Dtos
{
    public record RegisterRequest(
    [property: Required, EmailAddress, MaxLength(256)] string Email,
    [property: Required, MinLength(8), MaxLength(100)] string Password,
    [property: Required, StringLength(50, MinimumLength = 2)] string DisplayName);

    public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

    public record RefreshRequest(
    [property: Required] string RefreshToken);

    public record AuthResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken);
}
