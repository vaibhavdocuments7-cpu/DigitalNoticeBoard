using DigitalNoticeBoard.Domain.Entities;

namespace DigitalNoticeBoard.Application.Abstractions;

public interface INoticeRepository
{
    Task<IReadOnlyList<Notice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notice>> GetPublishedAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);
    Task<Notice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpsertAsync(Notice notice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
