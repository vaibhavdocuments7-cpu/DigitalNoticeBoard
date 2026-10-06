using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;
using DigitalNoticeBoard.Infrastructure.Memory;
using DigitalNoticeBoard.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace DigitalNoticeBoard.Infrastructure.Resilience;

public sealed class ResilientUserRepository(
    SqlUserRepository sqlRepository,
    InMemoryUserRepository memoryRepository,
    StorageState storageState,
    ILogger<ResilientUserRepository> logger) : IUserRepository
{
    public async Task<AppUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        if (!storageState.UseSqlServer)
        {
            return await memoryRepository.GetByUsernameAsync(username, cancellationToken);
        }

        try
        {
            var user = await sqlRepository.GetByUsernameAsync(username, cancellationToken);
            if (user is not null)
            {
                await memoryRepository.UpsertAsync(user, cancellationToken);
            }

            return user;
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            SwitchToFallback(exception);
            return await memoryRepository.GetByUsernameAsync(username, cancellationToken);
        }
    }

    public async Task UpsertAsync(
        AppUser user,
        CancellationToken cancellationToken = default)
    {
        if (!storageState.UseSqlServer)
        {
            await memoryRepository.UpsertAsync(user, cancellationToken);
            return;
        }

        try
        {
            await sqlRepository.UpsertAsync(user, cancellationToken);
            await memoryRepository.UpsertAsync(user, cancellationToken);
        }
        catch (Exception exception) when (SqlFailureDetector.IsDatabaseUnavailable(exception))
        {
            SwitchToFallback(exception);
            await memoryRepository.UpsertAsync(user, cancellationToken);
        }
    }

    private void SwitchToFallback(Exception exception)
    {
        logger.LogWarning(
            exception,
            "SQL Server became unavailable. Switching user storage to process memory.");
        storageState.SwitchToMemory(
            "SQL Server is unavailable. Authentication is using process-memory accounts.");
    }
}
