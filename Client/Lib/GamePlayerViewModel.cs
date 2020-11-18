using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Blazored.LocalStorage;
using System.Collections.Generic;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using System.Threading;

namespace MarketMakingGame.Client.Lib
{
  public class GamePlayerViewModel : BaseViewModel
  {
    private const int REQUEST_DELAY_MILLIS = 30000;

    public class GameAlertEventArgs : EventArgs
    {
      public bool IsError { get; set; }

      public string Title { get; set; }

      public string Message { get; set; }
    };

    private const int STATE_CREATED = 0, STATE_INITIALIZING = 1, STATE_INITIALIZED = 2;
    private volatile int state = STATE_CREATED;
    private ILocalStorageService LocalStorageService { get; }
    private ILogger Logger { get; }
    private NavigationManager NavigationManager { get; }
    private GameClient GameClient { get; }

    public bool IsInitialized => state == STATE_INITIALIZED;
    public string GameId { get; set; }
    public JoinGameResponse JoinGameResponse { get; set; }
    public PlayerUpdateResponse PlayerUpdateResponse { get; set; }
    public GameUpdateResponse GameUpdateResponse { get; set; }
    public List<Card> Cards { get; set; }
    public Card UnopenedCard { get; set; }

    public GamePlayerViewModel(ILocalStorageService localStorage,
      ILoggerProvider loggerProvider, NavigationManager navigationManager)
    {
      LocalStorageService = localStorage;
      Logger = loggerProvider.CreateLogger(nameof(GamePlayerViewModel));
      NavigationManager = navigationManager;
      GameClient = new GameClient(loggerProvider, navigationManager);
      GameClient.OnJoinGameResponse += HandleJoinGameResponse;
      GameClient.OnPlayerUpdateResponse += HandlePlayerUpdateResponse;
      GameClient.OnGameUpdateResponse += HandleGameUpdateResponse;
    }

    private void HandleGameUpdateResponse(GameUpdateResponse obj)
    {
      var isUpdated = this.GameUpdateResponse != null;
      this.GameUpdateResponse = obj;
      if (isUpdated)
        InvokeStateChanged(EventArgs.Empty);
    }

    private void HandlePlayerUpdateResponse(PlayerUpdateResponse obj)
    {
      var isUpdated = this.PlayerUpdateResponse != null;
      this.PlayerUpdateResponse = obj;
      if (isUpdated)
        InvokeStateChanged(EventArgs.Empty);
    }

    private void HandleJoinGameResponse(JoinGameResponse obj)
    {
      this.JoinGameResponse = obj;
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return (true, string.Empty);
    }

    public override async Task InitializeAsync()
    {
      if (Interlocked.CompareExchange(ref state, STATE_INITIALIZING, STATE_CREATED) != STATE_CREATED)
        return;
      await GameClient.InitializeAsync();
      var getCardsResp = await GameClient.InvokeRequestAsync<GetCardsResponse>("GetCards", new GetCardsRequest());
      if (getCardsResp.IsSuccess)
      {
        Cards = getCardsResp.Cards;
        UnopenedCard = getCardsResp.UnopenedCard;
      }
      else
      {
        throw new Exception(getCardsResp.ErrorMessage);
      }

      var joinReq = new JoinGameRequest();
      joinReq.GameId = GameId;
      joinReq.Player = await LocalStorageService.GetItemAsync<Player>(UserDataEditorViewModel.USER_DATA_KEY);
      await GameClient.SendRequestAsync("JoinGame", joinReq);
      await Task.Delay(REQUEST_DELAY_MILLIS);

      if (JoinGameResponse == null || PlayerUpdateResponse == null || GameUpdateResponse == null)
      {
        throw new TimeoutException();
      }

      if (!JoinGameResponse.IsSuccess)
      {
        throw new Exception(JoinGameResponse.ErrorMessage);
      }
      
      state = STATE_INITIALIZED;
      Logger.LogInformation("Init!");
      InvokeStateChanged(EventArgs.Empty);
    }

    public override void Dispose()
    {
      GameClient.Dispose();
      Logger.LogInformation("Dispose!");
    }
  }
}