using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;
using DigitalNoticeBoard.Infrastructure.Memory;
using DigitalNoticeBoard.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace DigitalNoticeBoard.Infrastructure.Resilience;

public sealed class ResilientNoticeRepository(
    SqlNoticeRepository sqlRepository,
    InMemoryNoticeRepository memoryRepository,
    StorageState storageState,
    ILogger<ResilientNoticeRepository> logger) : INoticeRepository
{
    public async Task<IReadOnlyList<Notice>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (!storageState.UseSqlServer)
        {
            return await memoryRepository.GetAllAsync(cancellationToken);
        }

        try
        {
            var notices = await sqlRepository.GetAllAsync(cancellationToken);
            await MirrorAsync(notices, cancellationToken);
            return notices;
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            SwitchToFallback(exception);
            return await memoryRepository.GetAllAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<Notice>> GetPublishedAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        if (!storageState.UseSqlServer)
        {
            return await memoryRepository.GetPublishedAsync(utcNow, cancellationToken);
        }

        try
        {
            var notices = await sqlRepository.GetPublishedAsync(utcNow, cancellationToken);
            await MirrorAsync(notices, cancellationToken);
            return notices;
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            SwitchToFallback(exception);
            return await memoryRepository.GetPublishedAsync(utcNow, cancellationToken);
        }
    }

    public async Task<Notice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!storageState.UseSqlServer)
        {
            return await memoryRepository.GetByIdAsync(id, cancellationToken);
        }

        try
        {
            var notice = await sqlRepository.GetByIdAsync(id, cancellationToken);
            if (notice is not null)
            {
                await memoryRepository.UpsertAsync(notice, cancellationToken);
            }

            return notice;
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            SwitchToFallback(exception);
            return await memoryRepository.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task UpsertAsync(
        Notice notice,
        CancellationToken cancellationToken = default)
    {
        if (!storageState.UseSqlServer)
        {
            await memoryRepository.UpsertAsync(notice, cancellationToken);
            return;
        }

        try
        {
            await sqlRepository.UpsertAsync(notice, cancellationToken);
            await memoryRepository.UpsertAsync(notice, cancellationToken);
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            SwitchToFallback(exception);
            await memoryRepository.UpsertAsync(notice, cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!storageState.UseSqlServer)
        {
            await memoryRepository.DeleteAsync(id, cancellationToken);
            return;
        }

        try
        {
            await sqlRepository.DeleteAsync(id, cancellationToken);
            await memoryRepository.DeleteAsync(id, cancellationToken);
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            SwitchToFallback(exception);
            await memoryRepository.DeleteAsync(id, cancellationToken);
        }
    }

    private async Task MirrorAsync(
        IEnumerable<Notice> notices,
        CancellationToken cancellationToken)
    {
        foreach (var notice in notices)
        {
            await memoryRepository.UpsertAsync(notice, cancellationToken);
        }
    }

    private void SwitchToFallback(Exception exception)
    {
        logger.LogWarning(
            exception,
            "SQL Server became unavailable. Switching notice storage to process memory.");
        storageState.SwitchToMemory(
            "SQL Server is unavailable. Changes are temporarily stored in process memory.");
    }
}
