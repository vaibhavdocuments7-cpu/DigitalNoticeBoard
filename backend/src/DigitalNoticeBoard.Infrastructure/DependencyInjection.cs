using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Infrastructure.Initialization;
using DigitalNoticeBoard.Infrastructure.Memory;
using DigitalNoticeBoard.Infrastructure.Persistence;
using DigitalNoticeBoard.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalNoticeBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<NoticeBoardDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<StorageState>();
        services.AddSingleton<IStorageStatus>(provider =>
            provider.GetRequiredService<StorageState>());
        services.AddSingleton<InMemoryNoticeRepository>();
        services.AddSingleton<InMemoryUserRepository>();

        services.AddScoped<SqlNoticeRepository>();
        services.AddScoped<SqlUserRepository>();
        services.AddScoped<INoticeRepository, ResilientNoticeRepository>();
        services.AddScoped<IUserRepository, ResilientUserRepository>();
        services.AddScoped<ApplicationDataInitializer>();

        return services;
    }
}
