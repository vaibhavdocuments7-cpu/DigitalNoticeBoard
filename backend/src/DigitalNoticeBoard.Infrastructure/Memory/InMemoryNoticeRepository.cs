using System.Collections.Concurrent;
using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;

namespace DigitalNoticeBoard.Infrastructure.Memory;

public sealed class InMemoryNoticeRepository : INoticeRepository
{
    private readonly ConcurrentDictionary<Guid, Notice> _notices = new();

    public Task<IReadOnlyList<Notice>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Notice> notices = _notices.Values
            .Select(Clone)
            .OrderByDescending(notice => notice.Priority)
            .ThenByDescending(notice => notice.UpdatedAtUtc)
            .ToList();
        return Task.FromResult(notices);
    }

    public Task<IReadOnlyList<Notice>> GetPublishedAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Notice> notices = _notices.Values
            .Where(notice => notice.IsPublished(utcNow))
            .Select(Clone)
            .OrderByDescending(notice => notice.Priority)
            .ThenByDescending(notice => notice.PublishFromUtc)
            .ToList();
        return Task.FromResult(notices);
    }

    public Task<Notice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = _notices.TryGetValue(id, out var notice)
            ? Clone(notice)
            : null;
        return Task.FromResult(result);
    }

    public Task UpsertAsync(Notice notice, CancellationToken cancellationToken = default)
    {
        _notices[notice.Id] = Clone(notice);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _notices.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private static Notice Clone(Notice notice)
    {
        return new Notice
        {
            Id = notice.Id,
            Title = notice.Title,
            Summary = notice.Summary,
            Content = notice.Content,
            Category = notice.Category,
            Priority = notice.Priority,
            PublishFromUtc = notice.PublishFromUtc,
            PublishUntilUtc = notice.PublishUntilUtc,
            Status = notice.Status,
            CreatedByUserId = notice.CreatedByUserId,
            CreatedAtUtc = notice.CreatedAtUtc,
            UpdatedAtUtc = notice.UpdatedAtUtc
        };
    }
}
