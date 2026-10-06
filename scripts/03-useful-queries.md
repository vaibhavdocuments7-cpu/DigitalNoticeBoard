# Useful SQL queries

## Currently visible notices

```sql
USE DigitalNoticeBoard;
GO

DECLARE @Now DATETIME2 = SYSUTCDATETIME();

SELECT
    Id,
    Title,
    Category,
    Priority,
    PublishFromUtc,
    PublishUntilUtc
FROM dbo.Notices
WHERE Status = N'Published'
  AND PublishFromUtc <= @Now
  AND (PublishUntilUtc IS NULL OR PublishUntilUtc > @Now)
ORDER BY Priority DESC, PublishFromUtc DESC;
```

## Expire completed notices

```sql
USE DigitalNoticeBoard;
GO

UPDATE dbo.Notices
SET
    Status = N'Archived',
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Status = N'Published'
  AND PublishUntilUtc IS NOT NULL
  AND PublishUntilUtc <= SYSUTCDATETIME();
```

## Notice counts by status and category

```sql
USE DigitalNoticeBoard;
GO

SELECT
    Status,
    Category,
    COUNT_BIG(*) AS NoticeCount
FROM dbo.Notices
GROUP BY Status, Category
ORDER BY Status, NoticeCount DESC;
```

## Disable a local account

```sql
USE DigitalNoticeBoard;
GO

UPDATE dbo.Users
SET IsActive = 0
WHERE Username = N'viewer';
```
