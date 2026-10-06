using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalNoticeBoard.Infrastructure.Resilience;

internal static class SqlFailureDetector
{
    public static bool IsDatabaseUnavailable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException or TimeoutException or PlatformNotSupportedException)
            {
                return true;
            }

            if (current is DbUpdateException)
            {
                continue;
            }

            if (current is InvalidOperationException
                && (current.Message.Contains("connection", StringComparison.OrdinalIgnoreCase)
                    || current.Message.Contains("LocalDB", StringComparison.OrdinalIgnoreCase)
                    || current.Message.Contains("server", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
