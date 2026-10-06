using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Constants;
using DigitalNoticeBoard.Domain.Entities;
using DigitalNoticeBoard.Domain.Enums;
using DigitalNoticeBoard.Infrastructure.Memory;
using DigitalNoticeBoard.Infrastructure.Persistence;
using DigitalNoticeBoard.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;

namespace DigitalNoticeBoard.Infrastructure.Initialization;

public sealed class ApplicationDataInitializer(
    NoticeBoardDbContext dbContext,
    SqlNoticeRepository sqlNotices,
    SqlUserRepository sqlUsers,
    InMemoryNoticeRepository memoryNotices,
    InMemoryUserRepository memoryUsers,
    IPasswordService passwordService,
    StorageState storageState,
    ILogger<ApplicationDataInitializer> logger)
{
    private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ViewerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public async Task InitializeAsync(
        string? connectionString,
        string? adminPassword,
        string? viewerPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(adminPassword)
            || string.IsNullOrWhiteSpace(viewerPassword))
        {
            throw new InvalidOperationException(
                "Demo account passwords must be configured through DemoAccounts settings.");
        }

        var users = CreateUsers(adminPassword, viewerPassword);
        var notices = CreateNotices();

        foreach (var user in users)
        {
            await memoryUsers.UpsertAsync(user, cancellationToken);
        }

        foreach (var notice in notices)
        {
            await memoryNotices.UpsertAsync(notice, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            storageState.SwitchToMemory(
                "No SQL connection string is configured. Data is stored in process memory.");
            return;
        }

        try
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            foreach (var user in users)
            {
                if (await sqlUsers.GetByUsernameAsync(user.Username, cancellationToken) is null)
                {
                    await sqlUsers.UpsertAsync(user, cancellationToken);
                }
            }

            if ((await sqlNotices.GetAllAsync(cancellationToken)).Count == 0)
            {
                foreach (var notice in notices)
                {
                    await sqlNotices.UpsertAsync(notice, cancellationToken);
                }
            }

            storageState.SwitchToSqlServer();
            logger.LogInformation("SQL Server is connected and initialized.");
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            logger.LogWarning(
                exception,
                "SQL Server initialization failed. The application will use process memory.");
            storageState.SwitchToMemory(
                "SQL Server could not be reached. Data is temporarily stored in process memory.");
        }
    }

    private IReadOnlyList<AppUser> CreateUsers(string adminPassword, string viewerPassword)
    {
        var admin = new AppUser
        {
            Id = AdminId,
            Username = "admin",
            DisplayName = "Notice Board Administrator",
            Role = AppRoles.Admin,
            IsActive = true
        };
        admin.PasswordHash = passwordService.Hash(admin, adminPassword);

        var viewer = new AppUser
        {
            Id = ViewerId,
            Username = "viewer",
            DisplayName = "Common Viewer",
            Role = AppRoles.Viewer,
            IsActive = true
        };
        viewer.PasswordHash = passwordService.Hash(viewer, viewerPassword);

        return [admin, viewer];
    }

    private static IReadOnlyList<Notice> CreateNotices()
    {
        var now = DateTime.UtcNow;
        return
        [
            new Notice
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                Title = "Welcome to the Digital Notice Board",
                Summary = "Important updates, events, and announcements in one place.",
                Content = "Use the viewer account to browse published notices. Administrators can create, schedule, publish, and archive announcements.",
                Category = "General",
                Priority = 5,
                PublishFromUtc = now.AddDays(-1),
                PublishUntilUtc = now.AddMonths(3),
                Status = NoticeStatus.Published,
                CreatedByUserId = AdminId,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Notice
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                Title = "Friday Community Meetup",
                Summary = "Join the monthly community meetup at 4:00 PM.",
                Content = "The meetup includes project demonstrations, team announcements, and an open question-and-answer session.",
                Category = "Events",
                Priority = 3,
                PublishFromUtc = now.AddHours(-2),
                PublishUntilUtc = now.AddDays(14),
                Status = NoticeStatus.Published,
                CreatedByUserId = AdminId,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        ];
    }
}
