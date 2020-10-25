using System;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace MarketMakingGame.Client.Lib
{
  class AppService : IDisposable
  {
    private const int STATE_CREATED = 0, STATE_INITIALIZING = 1, STATE_INITIALIZED = 2;
    private volatile int state = STATE_CREATED;
    private ILogger _logger;

    public UserDataEditorViewModel UserDataEditorViewModel { get; private set; }
    public GameClient GameClient { get; private set; }

    public AppService(ILocalStorageService localStorage,
    ILoggerProvider loggerProvider, NavigationManager navigationManager)
    {
      _logger = loggerProvider.CreateLogger("AppService");
      UserDataEditorViewModel = new UserDataEditorViewModel(localStorage);
      GameClient = new GameClient(loggerProvider, navigationManager);
      _logger.LogInformation("AppService Created!");
    }

    public async Task InitializeAsync()
    {
      if (Interlocked.CompareExchange(ref state, STATE_INITIALIZING, STATE_CREATED) != STATE_CREATED)
        return;
      await UserDataEditorViewModel.InitializeAsync();
      await GameClient.InitializeAsync();
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