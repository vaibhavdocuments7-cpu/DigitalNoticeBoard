using DigitalNoticeBoard.Application.Abstractions;

namespace DigitalNoticeBoard.Infrastructure.Resilience;

public sealed class StorageState : IStorageStatus
{
    private readonly object _sync = new();
    private bool _useSqlServer;
    private string _message = "SQL Server has not been checked; using process memory.";

    public string Provider
    {
        get
        {
            lock (_sync)
            {
                return _useSqlServer ? "SQL Server" : "In-memory";
            }
        }
    }

    public bool IsFallback
    {
        get
        {
            lock (_sync)
            {
                return !_useSqlServer;
            }
        }
    }

    public string Message
    {
        get
        {
            lock (_sync)
            {
                return _message;
            }
        }
    }

    internal bool UseSqlServer
    {
        get
        {
            lock (_sync)
            {
                return _useSqlServer;
            }
        }
    }

    public void SwitchToSqlServer()
    {
        lock (_sync)
        {
            _useSqlServer = true;
            _message = "SQL Server is connected and is the active repository.";
        }
    }

    public void SwitchToMemory(string reason)
    {
        lock (_sync)
        {
            _useSqlServer = false;
            _message = reason;
        }
    }
}
