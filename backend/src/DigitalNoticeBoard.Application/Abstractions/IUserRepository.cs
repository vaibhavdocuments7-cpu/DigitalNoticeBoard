using DigitalNoticeBoard.Domain.Entities;

namespace DigitalNoticeBoard.Application.Abstractions;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(AppUser user, CancellationToken cancellationToken = default);
}
