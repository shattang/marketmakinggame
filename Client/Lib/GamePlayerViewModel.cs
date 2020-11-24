using System;
using System.Linq;
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

    public class PlayerData
    {
      public string PlayerName { get; set; }
      public double Bid { get; set; }
      public double Offer { get; set; }
      public string PlayerImageUrl { get; set; }
      public double PositionQty { get; set; }
      public double PositionPrice { get; set; }
    }

    private UserDataEditorViewModel UserDataEditor { get; }
    private ILogger Logger { get; }
    private NavigationManager NavigationManager { get; }
    private GameClient GameClient { get; }
    public string GameId { get; set; }
    public JoinGameResponse JoinGameResponse { get; set; }
    public PlayerUpdateResponse PlayerUpdateResponse { get; set; }
    public GameUpdateResponse GameUpdateResponse { get; set; }
    public List<Card> Cards { get; set; }
    public Card UnopenedCard { get; set; }

    public GamePlayerViewModel(UserDataEditorViewModel localStorage, ILoggerProvider loggerProvider,
      NavigationManager navigationManager, GameClient gameClient)
    {
      UserDataEditor = localStorage;
      Logger = loggerProvider.CreateLogger(nameof(GamePlayerViewModel));
      NavigationManager = navigationManager;
      GameClient = gameClient;
      GameClient.OnJoinGameResponse += HandleJoinGameResponse;
      GameClient.OnPlayerUpdateResponse += HandlePlayerUpdateResponse;
      GameClient.OnGameUpdateResponse += HandleGameUpdateResponse;
    }

    private void HandleGameUpdateResponse(GameUpdateResponse obj)
    {
      var isUpdated = this.GameUpdateResponse != null;
      this.GameUpdateResponse = obj;
      SetInitializedIfReady();
      InvokeStateChanged(EventArgs.Empty);
    }

    private void HandlePlayerUpdateResponse(PlayerUpdateResponse obj)
    {
      var isUpdated = this.PlayerUpdateResponse != null;
      this.PlayerUpdateResponse = obj;
      SetInitializedIfReady();
      InvokeStateChanged(EventArgs.Empty);
    }

    private void HandleJoinGameResponse(JoinGameResponse obj)
    {
      this.JoinGameResponse = obj;
      SetInitializedIfReady();
      InvokeStateChanged(EventArgs.Empty);
    }

    private void SetInitializedIfReady()
    {
      if (JoinGameResponse == null || PlayerUpdateResponse == null || GameUpdateResponse == null)
      {
        return;
      }
      state = STATE_INITIALIZED;
      Logger.LogInformation("Init!");
    }

    protected override async Task InitializeAsync()
    {
      await GameClient.InitViewModelAsync();
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
      joinReq.Player = UserDataEditor.Data;
      await GameClient.SendRequestAsync("JoinGame", joinReq);
    }

    public string GetCommunityCardImageUrl(int index)
    {
      if (index < GameUpdateResponse.CommunityCardIds.Count)
      {
        var cardId = GameUpdateResponse.CommunityCardIds[index];
        return Cards.Where(x => x.CardId == cardId).DefaultIfEmpty(UnopenedCard).First().CardImageUrl;
      }
      return UnopenedCard.CardImageUrl;
    }

    public string GamePlayerCardImageUrl()
    {
      var cardId = PlayerUpdateResponse.CardId;
      return Cards.Where(x => x.CardId == cardId).DefaultIfEmpty(UnopenedCard).First().CardImageUrl;
    }

    public double BestBid
    {
      get
      {
        return GameUpdateResponse != null && GameUpdateResponse.BestCurrentBid.HasValue ? GameUpdateResponse.BestCurrentBid.Value : double.NaN;
      }
    }

    public double BestAsk
    {
      get
      {
        return GameUpdateResponse != null && GameUpdateResponse.BestCurrentAsk.HasValue ? GameUpdateResponse.BestCurrentAsk.Value : double.NaN;
      }
    }

    public double GetCardValue(int cardId)
    {
      return Cards.Where(x => x.CardId == cardId).DefaultIfEmpty(UnopenedCard).Select(x => x.CardValue).First();
    }

    public double IndicativePrice
    {
      get
      {
        if (GameUpdateResponse != null && GameUpdateResponse.CommunityCardIds != null)
        {
          var sum = GameUpdateResponse.CommunityCardIds
            .Select(GetCardValue)
            .Sum();
          sum += GetCardValue(PlayerUpdateResponse?.CardId ?? UnopenedCard.CardId);
          return sum;
        }
        return double.NaN;
      }
    }

    public IEnumerable<PlayerData> PlayersData
    {
      get
      {
        if (GameUpdateResponse != null)
        {
          var myPlayerId = PlayerUpdateResponse != null ? PlayerUpdateResponse.PlayerPublicId : -1;
          return GameUpdateResponse.PlayerPublicStates.Select(x =>
          {
            return new PlayerData()
            {
              PlayerName = x.PlayerPublicId == myPlayerId ? $"{x.DisplayName} (You)" : x.DisplayName,
              PlayerImageUrl = UserDataEditorViewModel.ToPlayerAvatarUrl(x.AvatarSeed),
              Bid = x.CurrentBid.HasValue ? x.CurrentBid.Value : double.NaN,
              Offer = x.CurrentAsk.HasValue ? x.CurrentAsk.Value : double.NaN,
              PositionPrice = x.PositionCashFlow.HasValue && x.PositionQty > 0 ? x.PositionCashFlow.Value / x.PositionQty.Value : double.NaN,
              PositionQty = x.PositionQty.HasValue ? x.PositionQty.Value : double.NaN
            };
          }).OrderBy(x => x.PlayerName);
        }
        return Enumerable.Empty<PlayerData>();
      }
    }

    public string FormatPrice(double val)
    {
      return Double.IsNaN(val) ? "-" : String.Format("${0:f2}", val);
    }

    public string FormatQty(double val)
    {
      return Double.IsNaN(val) ? "-" : String.Format("${0:f2}", val);
    }

    public double BidPrice
    {
      get;
      set;
    }

    public double AskPrice
    {
      get;
      set;
    }

    public override void Dispose()
    {
      GameClient.OnJoinGameResponse -= HandleJoinGameResponse;
      GameClient.OnPlayerUpdateResponse -= HandlePlayerUpdateResponse;
      GameClient.OnGameUpdateResponse -= HandleGameUpdateResponse;
      Logger.LogInformation("Dispose!");
    }
  }
}