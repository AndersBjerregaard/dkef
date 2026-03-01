using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts;

public sealed record RefreshTokenDto(
    [Required]
    string RefreshToken
);
