namespace DigitalNoticeBoard.Application.Abstractions;

public interface IStorageStatus
{
    string Provider { get; }
    bool IsFallback { get; }
    string Message { get; }
}
