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
      public bool IsModal { get; set; }
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
      public bool IsConnected { get; set; }
    }

    public class TradeData
    {
      public string InitiatingPlayerName { get; set; }
      public string TargetPlayerName { get; set; }
      public double TradeQty { get; set; }
      public double TradePrice { get; set; }
    }

    private UserDataEditorViewModel UserDataEditor { get; }
    private ILogger Logger { get; }
    private NavigationManager NavigationManager { get; }
    private GameClient GameClient { get; }
    public string GameId { get; set; }
    public JoinGameResponse JoinGameResponse { get; set; }
    public PlayerUpdateResponse PlayerUpdateResponse { get; set; }
    public GameUpdateResponse GameUpdateResponse { get; set; }
    private Dictionary<int, TradeUpdate> Trades { get; } = new Dictionary<int, TradeUpdate>();
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
      GameClient.OnIsConnectedChanged += HandleIsConnectedChanged;
      GameClient.OnTradeUpdateResponse += HandleTradeUpdateResponse;
      GameClient.OnUpdateQuoteResponse += HandleUpdateQuoteResponse;
      GameClient.OnDealGameResponse += HandleDealGameResponse;
      GameClient.OnTradeResponse += HandleTradeResponse;
    }

    private void HandleTradeResponse(TradeResponse obj)
    {
      if (!obj.IsSuccess)
      {
        InvokeStateChanged(new GameAlertEventArgs()
        {
          IsModal = true,
          IsError = true,
          Title = "Failed to Initate Trade",
          Message = $"Error: {obj.ErrorMessage}"
        });
      }
    }

    private void HandleDealGameResponse(DealGameResponse obj)
    {
      if (!obj.IsSuccess)
      {
        InvokeStateChanged(new GameAlertEventArgs()
        {
          IsModal = true,
          IsError = true,
          Title = "Failed to Deal",
          Message = $"Error: {obj.ErrorMessage}"
        });
      }
    }

    private void HandleUpdateQuoteResponse(UpdateQuoteResponse obj)
    {
      if (!obj.IsSuccess)
      {
        InvokeStateChanged(new GameAlertEventArgs()
        {
          IsModal = true,
          IsError = true,
          Title = "Failed to Update Quote",
          Message = $"Error: {obj.ErrorMessage}"
        });
      }
    }

    private void HandleTradeUpdateResponse(TradeUpdateResponse obj)
    {
      foreach (var t in obj.TradeUpdates)
      {
        Trades[t.TradeId] = t;
      }
      InvokeStateChanged(new GameAlertEventArgs()
      {
        Title = "New trades were processed"
      });
    }

    private void HandleIsConnectedChanged(bool isConnected)
    {
      if (isConnected)
      {
        if (IsInitialized)
        {
          _ = SendJoinRequest();
          InvokeStateChanged(new GameAlertEventArgs()
          {
            Title = "Reconnected",
            Message = "Rejoining game ..."
          });
        }
      }
      else
      {
        if (IsInitialized)
        {
          InvokeStateChanged(new GameAlertEventArgs()
          {
            IsError = true,
            Title = "Disconnected from Server",
            Message = "Attempting to reconnect ..."
          });
        }
      }
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
      if (JoinGameResponse != null)
      {
        InvokeStateChanged(new GameAlertEventArgs()
        {
          Title = "Rejoined Game",
          Message = string.Empty
        });
      }

      this.JoinGameResponse = obj;
      SetInitializedIfReady();
      InvokeStateChanged(EventArgs.Empty);
    }

    private void SetInitializedIfReady()
    {
      if (IsInitialized || JoinGameResponse == null || PlayerUpdateResponse == null || GameUpdateResponse == null)
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

      await SendJoinRequest();
    }

    private async Task SendJoinRequest()
    {
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
        return GameUpdateResponse != null &&
          GameUpdateResponse.BestCurrentBid.HasValue ? GameUpdateResponse.BestCurrentBid.Value : double.NaN;
      }
    }

    public double BestAsk
    {
      get
      {
        return GameUpdateResponse != null &&
          GameUpdateResponse.BestCurrentAsk.HasValue ? GameUpdateResponse.BestCurrentAsk.Value : double.NaN;
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
              PositionQty = x.PositionQty.HasValue ? x.PositionQty.Value : double.NaN,
              IsConnected = x.IsConnected
            };
          }).OrderBy(x => x.PlayerName);
        }
        return Enumerable.Empty<PlayerData>();
      }
    }

    public IEnumerable<TradeData> TradesData
    {
      get
      {
        PlayerPublicState getPlayer(int playerId)
        {
          if (GameUpdateResponse != null)
          {
            return GameUpdateResponse.PlayerPublicStates
              .FirstOrDefault(x => x.PlayerPublicId == playerId);
          }
          return null;
        }

        return Trades.Values.OrderBy(x => x.TradeId).Select(x =>
        {
          var initiator = getPlayer(x.InitiatorPlayerPublicId);
          var target = getPlayer(x.TargetPlayerPublicId);
          return new TradeData()
          {
            InitiatingPlayerName = initiator?.DisplayName ?? string.Empty,
            TargetPlayerName = target?.DisplayName ?? string.Empty,
            TradePrice = x.TradePrice,
            TradeQty = x.TradeQty
          };
        });
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

    private bool CheckIsConnected(string action)
    {
      if (!GameClient.IsConnected)
      {
        InvokeStateChanged(new GameAlertEventArgs()
        {
          IsError = true,
          Title = $"Unable to perform {action}",
          Message = "Not connected to Server"
        });
        return false;
      }
      return true;
    }

    public void SendUpdateQuoteRequest()
    {
      if (!CheckIsConnected("UpdateQuote"))
      {
        return;
      }

      GameClient.SendRequestAsync("UpdateQuote", new UpdateQuoteRequest()
      {
        CurrentAsk = AskPrice,
        CurrentBid = BidPrice,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
    }

    public void SendTradeRequest(bool isBuy)
    {
      if (!CheckIsConnected("Trade"))
      {
        return;
      }

      GameClient.SendRequestAsync("Trade", new TradeRequest()
      {
        IsBuy = isBuy,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
    }

    public void SendDealRequest()
    {
      var request = GameUpdateResponse.AllRoundsFinished ? 
        DealGameRequest.RequestTypes.FinishGame : DealGameRequest.RequestTypes.DealCard;

      if (!CheckIsConnected(request.ToString()))
      {
        return;
      }

      GameClient.SendRequestAsync("DealGame", new DealGameRequest()
      {
        RequestType = request,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
    }

    public void SendLockTradingRequest(bool block)
    {
      var request = block ? DealGameRequest.RequestTypes.LockTrading : DealGameRequest.RequestTypes.UnlockTrading;
      if (!CheckIsConnected(request.ToString()))
      {
        return;
      }

      GameClient.SendRequestAsync("DealGame", new DealGameRequest()
      {
        RequestType = request,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
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