using System;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace MarketMakingGame.Client.Lib
{
  public class AppViewModel : IDisposable
  {
    private const int STATE_CREATED = 0, STATE_INITIALIZING = 1, STATE_INITIALIZED = 2;
    private volatile int state = STATE_CREATED;
    private ILogger _logger;

    public GameClient GameClient { get; }
    public UserDataEditorViewModel UserDataEditorViewModel { get; }
    public GameManagerViewModel GameManagerViewModel { get; }

    public AppViewModel(ILocalStorageService localStorage,
    ILoggerProvider loggerProvider, NavigationManager navigationManager)
    {
      _logger = loggerProvider.CreateLogger("AppService");
      GameClient = new GameClient(loggerProvider, navigationManager);
      UserDataEditorViewModel = new UserDataEditorViewModel(localStorage);
      GameManagerViewModel = new GameManagerViewModel(this, localStorage);
      _logger.LogInformation("AppService Created!");
    }

    public async Task InitializeAsync()
    {
      if (Interlocked.CompareExchange(ref state, STATE_INITIALIZING, STATE_CREATED) != STATE_CREATED)
        return;
      await GameClient.InitializeAsync();
      await UserDataEditorViewModel.InitializeAsync();
      await GameManagerViewModel.InitializeAsync();
      state = STATE_INITIALIZED;
      _logger.LogInformation("AppService Init!");
    }

    public void Dispose()
    {
      GameClient.Dispose();
      _logger.LogInformation("AppService Dispose!");
    }
  }
}