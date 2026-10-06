using DigitalNoticeBoard.Domain.Enums;

namespace DigitalNoticeBoard.Application.Notices;

public sealed record NoticeDto(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    string Category,
    int Priority,
    DateTime PublishFromUtc,
    DateTime? PublishUntilUtc,
    NoticeStatus Status,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record SaveNoticeRequest(
    string Title,
    string Summary,
    string Content,
    string Category,
    int Priority,
    DateTime PublishFromUtc,
    DateTime? PublishUntilUtc,
    NoticeStatus Status);
