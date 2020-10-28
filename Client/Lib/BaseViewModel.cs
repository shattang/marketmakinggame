using System;
using System.Threading.Tasks;

namespace MarketMakingGame.Client.Lib
{
  public abstract class BaseViewModel : IDisposable
  {
    public BaseViewModel()
    {
    }

    protected void InvokeStateChanged(EventArgs eventArgs)
    {
      if (StateChanged != null)
        StateChanged(eventArgs);
    }

    public abstract (bool Success, string ErrorMessages) CheckValid();

    public abstract Task InitializeAsync();

    public abstract void Dispose();

    public event Action<EventArgs> StateChanged;
  }
}