namespace Firmeza.Application.DTOs;

public record LoginRequestDto(
    string EmailOrUsername,
    string Password
);

public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    string? Role
);

public record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    IEnumerable<string> Roles,
    DateTime ExpiresAt
);
