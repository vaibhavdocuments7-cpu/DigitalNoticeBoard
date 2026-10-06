using System.Collections.Concurrent;
using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;

namespace DigitalNoticeBoard.Infrastructure.Memory;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, AppUser> _users =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<AppUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var result = _users.TryGetValue(username, out var user)
            ? Clone(user)
            : null;
        return Task.FromResult(result);
    }

    public Task UpsertAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        _users[user.Username] = Clone(user);
        return Task.CompletedTask;
    }

    private static AppUser Clone(AppUser user)
    {
        return new AppUser
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}
