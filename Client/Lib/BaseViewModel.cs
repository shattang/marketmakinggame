using System;
using System.Threading.Tasks;

namespace MarketMakingGame.Client.Lib
{
  public abstract class BaseViewModel
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

    public event Action<EventArgs> StateChanged;
  }
}