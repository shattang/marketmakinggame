using System;
using System.Threading;
using System.Threading.Tasks;

namespace MarketMakingGame.Client.Lib
{
  public abstract class BaseViewModel : IDisposable
  {
    protected const int STATE_CREATED = 0, STATE_INITIALIZING = 1, STATE_INITIALIZED = 2;
    protected volatile int state = STATE_CREATED;

    public bool IsInitialized => state == STATE_INITIALIZED;

    public BaseViewModel()
    {
    }

    protected void InvokeStateChanged(EventArgs eventArgs)
    {
      if (StateChanged != null)
        StateChanged(eventArgs);
    }

    public virtual (bool Success, string ErrorMessages) CheckValid()
    {
      return (true, string.Empty);
    }

    public async Task InitViewModelAsync()
    {
      if (Interlocked.CompareExchange(ref state, STATE_INITIALIZING, STATE_CREATED) != STATE_CREATED)
        return;
      await InitializeAsync();
      InvokeStateChanged(EventArgs.Empty);
    }

    protected abstract Task InitializeAsync();

    public abstract void Dispose();

    public event Action<EventArgs> StateChanged;
  }
}