using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DigitalNoticeBoard.Api.Authentication;

public sealed class AspNetPasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _hasher = new();

    public string Hash(AppUser user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public bool Verify(AppUser user, string password)
    {
        return _hasher.VerifyHashedPassword(user, user.PasswordHash, password)
            != PasswordVerificationResult.Failed;
    }
}
