using DigitalNoticeBoard.Application.Authentication;
using DigitalNoticeBoard.Domain.Entities;

namespace DigitalNoticeBoard.Application.Abstractions;

public interface IPasswordService
{
    string Hash(AppUser user, string password);
    bool Verify(AppUser user, string password);
}

public interface ITokenService
{
    TokenResult Create(AppUser user);
}
