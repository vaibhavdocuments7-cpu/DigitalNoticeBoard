using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalNoticeBoard.Infrastructure.Persistence;

public sealed class SqlUserRepository(NoticeBoardDbContext dbContext) : IUserRepository
{
    public Task<AppUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Username == username,
                cancellationToken);
    }

    public async Task UpsertAsync(
        AppUser user,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(
            existing => existing.Id == user.Id,
            cancellationToken))
        {
            dbContext.Users.Update(user);
        }
        else
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
