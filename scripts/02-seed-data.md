# Insert the initial administrator and sample notices

Set `@AdminPasswordHash` to an ASP.NET Core `PasswordHasher<AppUser>` hash for
the initial administrator password. A plaintext value will not work with the
API login.

```sql
USE DigitalNoticeBoard;
GO

DECLARE @AdminId UNIQUEIDENTIFIER =
    '11111111-1111-1111-1111-111111111111';
DECLARE @AdminPasswordHash NVARCHAR(512) =
    N'REPLACE_WITH_ASPNET_PASSWORD_HASH';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF @AdminPasswordHash = N'REPLACE_WITH_ASPNET_PASSWORD_HASH'
BEGIN
    THROW 50001, 'Set @AdminPasswordHash before running this script.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Users
    WHERE Id = @AdminId OR Username = N'admin'
)
BEGIN
    INSERT INTO dbo.Users
    (
        Id,
        Username,
        DisplayName,
        PasswordHash,
        Role,
        IsActive
    )
    VALUES
    (
        @AdminId,
        N'admin',
        N'Notice Board Administrator',
        @AdminPasswordHash,
        N'Admin',
        1
    );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Notices
    WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1'
)
BEGIN
    INSERT INTO dbo.Notices
    (
        Id,
        Title,
        Summary,
        Content,
        Category,
        Priority,
        PublishFromUtc,
        PublishUntilUtc,
        Status,
        CreatedByUserId,
        CreatedAtUtc,
        UpdatedAtUtc
    )
    VALUES
    (
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
        N'Welcome to the Digital Notice Board',
        N'Important updates, events, and announcements in one place.',
        N'Use the viewer account to browse notices. Administrators can create, schedule, publish, and archive announcements.',
        N'General',
        5,
        DATEADD(DAY, -1, @Now),
        DATEADD(MONTH, 3, @Now),
        N'Published',
        @AdminId,
        @Now,
        @Now
    );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Notices
    WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
)
BEGIN
    INSERT INTO dbo.Notices
    (
        Id,
        Title,
        Summary,
        Content,
        Category,
        Priority,
        PublishFromUtc,
        PublishUntilUtc,
        Status,
        CreatedByUserId,
        CreatedAtUtc,
        UpdatedAtUtc
    )
    VALUES
    (
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
        N'Friday Community Meetup',
        N'Join the monthly community meetup at 4:00 PM.',
        N'The meetup includes demonstrations, announcements, and an open question-and-answer session.',
        N'Events',
        3,
        DATEADD(HOUR, -2, @Now),
        DATEADD(DAY, 14, @Now),
        N'Published',
        @AdminId,
        @Now,
        @Now
    );
END;
GO
```
