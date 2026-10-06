using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;
using DigitalNoticeBoard.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalNoticeBoard.Infrastructure.Persistence;

public sealed class SqlNoticeRepository(NoticeBoardDbContext dbContext) : INoticeRepository
{
    public async Task<IReadOnlyList<Notice>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Notices
            .AsNoTracking()
            .OrderByDescending(notice => notice.Priority)
            .ThenByDescending(notice => notice.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notice>> GetPublishedAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Notices
            .AsNoTracking()
            .Where(notice =>
                notice.Status == NoticeStatus.Published
                && notice.PublishFromUtc <= utcNow
                && (notice.PublishUntilUtc == null || notice.PublishUntilUtc > utcNow))
            .OrderByDescending(notice => notice.Priority)
            .ThenByDescending(notice => notice.PublishFromUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Notice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Notices
            .AsNoTracking()
            .SingleOrDefaultAsync(notice => notice.Id == id, cancellationToken);
    }

    public async Task UpsertAsync(
        Notice notice,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Notices.AnyAsync(
            existing => existing.Id == notice.Id,
            cancellationToken))
        {
            dbContext.Notices.Update(notice);
        }
        else
        {
            await dbContext.Notices.AddAsync(notice, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notice = await dbContext.Notices.FindAsync([id], cancellationToken);
        if (notice is null)
        {
            return;
        }

        dbContext.Notices.Remove(notice);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
