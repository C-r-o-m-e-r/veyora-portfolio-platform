using Microsoft.JSInterop;

namespace Portfolio.Services;

public interface IBodyScrollLockService
{
    Task LockAsync();
    Task UnlockAsync();
}

public sealed class BodyScrollLockService(IJSRuntime jsRuntime) : IBodyScrollLockService
{
    private int _lockCount;

    public async Task LockAsync()
    {
        _lockCount++;
        if (_lockCount == 1)
        {
            await jsRuntime.InvokeVoidAsync("bodyScrollLock.lock");
        }
    }

    public async Task UnlockAsync()
    {
        if (_lockCount == 0)
        {
            return;
        }

        _lockCount--;
        if (_lockCount == 0)
        {
            await jsRuntime.InvokeVoidAsync("bodyScrollLock.unlock");
        }
    }
}
