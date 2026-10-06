namespace DigitalNoticeBoard.Application.Authentication;

public sealed record LoginRequest(string Username, string Password);

public sealed record UserDto(Guid Id, string Username, string DisplayName, string Role);

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc, UserDto User);

public sealed record TokenResult(string Token, DateTime ExpiresAtUtc);
