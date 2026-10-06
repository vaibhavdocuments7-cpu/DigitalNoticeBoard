namespace DigitalNoticeBoard.Api.Authentication;

public sealed record JwtSettings(
    string Key,
    string Issuer,
    string Audience,
    int ExpiryMinutes);
