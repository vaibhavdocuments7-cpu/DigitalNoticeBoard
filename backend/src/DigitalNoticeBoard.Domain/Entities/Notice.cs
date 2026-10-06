using DigitalNoticeBoard.Domain.Enums;

namespace DigitalNoticeBoard.Domain.Entities;

public sealed class Notice
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public int Priority { get; set; } = 1;
    public DateTime PublishFromUtc { get; set; }
    public DateTime? PublishUntilUtc { get; set; }
    public NoticeStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public bool IsPublished(DateTime utcNow)
    {
        return Status == NoticeStatus.Published
            && PublishFromUtc <= utcNow
            && (PublishUntilUtc is null || PublishUntilUtc > utcNow);
    }
}
