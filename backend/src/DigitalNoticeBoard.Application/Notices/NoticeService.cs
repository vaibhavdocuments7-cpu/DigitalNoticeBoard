using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;

namespace DigitalNoticeBoard.Application.Notices;

public sealed class NoticeService(INoticeRepository noticeRepository)
{
    public async Task<IReadOnlyList<NoticeDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var notices = await noticeRepository.GetAllAsync(cancellationToken);
        return notices.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<NoticeDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var notices = await noticeRepository.GetPublishedAsync(
            DateTime.UtcNow,
            cancellationToken);
        return notices.Select(ToDto).ToList();
    }

    public async Task<NoticeDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var notice = await noticeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Notice was not found.");
        return ToDto(notice);
    }

    public async Task<NoticeDto> CreateAsync(
        SaveNoticeRequest request,
        Guid createdByUserId,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var now = DateTime.UtcNow;
        var notice = new Notice
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        Apply(notice, request);
        await noticeRepository.UpsertAsync(notice, cancellationToken);
        return ToDto(notice);
    }

    public async Task<NoticeDto> UpdateAsync(
        Guid id,
        SaveNoticeRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var notice = await noticeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Notice was not found.");

        Apply(notice, request);
        notice.UpdatedAtUtc = DateTime.UtcNow;
        await noticeRepository.UpsertAsync(notice, cancellationToken);
        return ToDto(notice);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await noticeRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            throw new KeyNotFoundException("Notice was not found.");
        }

        await noticeRepository.DeleteAsync(id, cancellationToken);
    }

    private static void Validate(SaveNoticeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (request.Title.Trim().Length > 160)
        {
            throw new ArgumentException("Title cannot exceed 160 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required.");
        }

        if (request.Priority is < 1 or > 5)
        {
            throw new ArgumentException("Priority must be between 1 and 5.");
        }

        var publishFrom = ToUtc(request.PublishFromUtc);
        DateTime? publishUntil = request.PublishUntilUtc is null
            ? null
            : ToUtc(request.PublishUntilUtc.Value);

        if (publishUntil <= publishFrom)
        {
            throw new ArgumentException("Publish-until time must be after publish-from time.");
        }
    }

    private static void Apply(Notice notice, SaveNoticeRequest request)
    {
        notice.Title = request.Title.Trim();
        notice.Summary = request.Summary?.Trim() ?? string.Empty;
        notice.Content = request.Content.Trim();
        notice.Category = string.IsNullOrWhiteSpace(request.Category)
            ? "General"
            : request.Category.Trim();
        notice.Priority = request.Priority;
        notice.PublishFromUtc = ToUtc(request.PublishFromUtc);
        notice.PublishUntilUtc = request.PublishUntilUtc is null
            ? null
            : ToUtc(request.PublishUntilUtc.Value);
        notice.Status = request.Status;
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static NoticeDto ToDto(Notice notice)
    {
        return new NoticeDto(
            notice.Id,
            notice.Title,
            notice.Summary,
            notice.Content,
            notice.Category,
            notice.Priority,
            notice.PublishFromUtc,
            notice.PublishUntilUtc,
            notice.Status,
            notice.CreatedByUserId,
            notice.CreatedAtUtc,
            notice.UpdatedAtUtc);
    }
}
