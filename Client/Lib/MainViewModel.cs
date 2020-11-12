using System;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace MarketMakingGame.Client.Lib
{
  public class MainViewModel : BaseViewModel
  {
    public enum ViewTypes
    {
      Default,
      GameManager,
      GamePlayer
    };

    private const int STATE_CREATED = 0, STATE_INITIALIZING = 1, STATE_INITIALIZED = 2;
    private volatile int state = STATE_CREATED;
    private ILogger _logger;
    private ViewTypes viewType;

    public GameClient GameClient { get; }
    public UserDataEditorViewModel UserDataEditorViewModel { get; }
    public GameManagerViewModel GameManagerViewModel { get; }
    public GamePlayerViewModel GamePlayerViewModel { get; }

    public MainViewModel(ILocalStorageService localStorage,
    ILoggerProvider loggerProvider, NavigationManager navigationManager)
    {
      _logger = loggerProvider.CreateLogger(nameof(MainViewModel));
      GameClient = new GameClient(loggerProvider, navigationManager);
      UserDataEditorViewModel = new UserDataEditorViewModel(localStorage);
      GameManagerViewModel = new GameManagerViewModel(this, localStorage);
      GamePlayerViewModel = new GamePlayerViewModel(this);
      _logger.LogInformation("Created!");
    }

    public ViewTypes ViewType
    {
      get => viewType;
      set
      {
        viewType = value;
        InvokeStateChanged(EventArgs.Empty);
      }
    }

    public override async Task InitializeAsync()
    {
      if (Interlocked.CompareExchange(ref state, STATE_INITIALIZING, STATE_CREATED) != STATE_CREATED)
        return;
      await GameClient.InitializeAsync();
      await UserDataEditorViewModel.InitializeAsync();
      await GameManagerViewModel.InitializeAsync();
      await GamePlayerViewModel.InitializeAsync();
      state = STATE_INITIALIZED;
      _logger.LogInformation("Init!");
    }

    public override void Dispose()
    {
      GameClient.Dispose();
      _logger.LogInformation("Dispose!");
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return (true, string.Empty);
    }

    internal void ShowGamePlayer(Game game)
    {
      GamePlayerViewModel.CurrentGame = game;
      ViewType = ViewTypes.GamePlayer;
    }
  }
}