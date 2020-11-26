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
using Microsoft.JSInterop;

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
      public double CurrentPnl { get; set; }
      public double SettlementPnl { get; set; }
      public bool IsConnected { get; set; }
      public string PlayerCardImageUrl { get; set; }
    }

    public class TradeData
    {
      public int TradeSequence { get; set; }
      public string BuyerName { get; set; }
      public string SellerName { get; set; }
      public double TradeQty { get; set; }
      public double TradePrice { get; set; }
    }

    private UserDataEditorViewModel UserDataEditor { get; }
    private ILogger Logger { get; }
    private NavigationManager NavigationManager { get; }
    private GameClient GameClient { get; }
    public IJSRuntime JSRuntime { get; }
    public string GameId { get; set; }
    public JoinGameResponse JoinGameResponse { get; set; }
    public PlayerUpdateResponse PlayerUpdateResponse { get; set; }
    public GameUpdateResponse GameUpdateResponse { get; set; }
    private Dictionary<int, TradeUpdate> Trades { get; } = new Dictionary<int, TradeUpdate>();
    public List<Card> Cards { get; set; }
    public Card UnopenedCard { get; set; }

    public GamePlayerViewModel(UserDataEditorViewModel localStorage, ILoggerProvider loggerProvider,
      NavigationManager navigationManager, GameClient gameClient, IJSRuntime jSRuntime)
    {
      UserDataEditor = localStorage;
      Logger = loggerProvider.CreateLogger(nameof(GamePlayerViewModel));
      NavigationManager = navigationManager;
      GameClient = gameClient;
      JSRuntime = jSRuntime;
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
      if (!String.IsNullOrWhiteSpace(obj.Message))
      {
        InvokeStateChanged(new GameAlertEventArgs()
        {
          Title = "Issues with Update Quote",
          Message = obj.Message
        });
      }

      if (!obj.IsSuccess)
      {
        InvokeStateChanged(new GameAlertEventArgs()
        {
          IsModal = true,
          IsError = true,
          Title = "Failed to Update Quote",
          Message = $"Error: {obj.ErrorMessage}"
        });
        return;
      }

      this.BidPrice = obj.BidPrice ?? this.BidPrice;
      this.AskPrice = obj.AskPrice ?? this.AskPrice;
    }

    private void HandleTradeUpdateResponse(TradeUpdateResponse obj)
    {
      if (Trades.Count > 0)
      {
        if (PlayerUpdateResponse != null &&
            obj.TradeUpdates.Any(x => x.TargetPlayerPublicId == PlayerUpdateResponse.PlayerPublicId))
        {
          InvokeStateChanged(new GameAlertEventArgs()
          {
            Message = "Your Quotes were reset because there were Trades against them.",
            Title = "Please Update your Quotes"
          });
        }
        else
        {
          InvokeStateChanged(new GameAlertEventArgs()
          {
            Message = string.Empty,
            Title = "New trades were processed"
          });
        }
      }

      foreach (var t in obj.TradeUpdates)
      {
        Trades[t.TradeId] = t;
      }
      InvokeStateChanged(EventArgs.Empty);
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
        Trades.Clear();
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
      var prev = this.GameUpdateResponse;
      var isUpdated = this.GameUpdateResponse != null;
      this.GameUpdateResponse = obj;

      if (isUpdated)
      {
        var alerts = new List<string>();
        foreach (var item in obj.PlayerPublicStates)
        {
          var isConnected = item.IsConnected;
          var wasConnected = prev.PlayerPublicStates
            .FirstOrDefault(x => x.PlayerPublicId == item.PlayerPublicId)?.IsConnected ?? false;
          if (wasConnected != isConnected)
          {
            alerts.Add($"Player {item.DisplayName} {(isConnected ? "Connected" : "Disconnected")}");
          }
        }

        if (alerts.Count > 0)
        {
          InvokeStateChanged(new GameAlertEventArgs()
          {
            Message = String.Join(Environment.NewLine, alerts),
            Title = "Players Alert"
          });
        }
      }

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
      return GetCardImageUrl(cardId);
    }

    private string GetCardImageUrl(int cardId)
    {
      return Cards.Where(x => x.CardId == cardId).DefaultIfEmpty(UnopenedCard).First().CardImageUrl;
    }

    public string CurrentGameStatus
    {
      get
      {
        if (!GameClient.IsConnected)
        {
          return "Not Connected";
        }
        else
        {
          if (GameUpdateResponse == null)
            return string.Empty;

          if (GameUpdateResponse.IsFinished)
          {
            return $"Game Finished. Settlement Price is {FormatPrice(GameUpdateResponse.SettlementPrice)}";
          }
          else
          {
            return $"Game in Progress. Current Indicative Price is {FormatPrice(IndicativePrice)}";
          }
        }
      }
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
            var data = new PlayerData()
            {
              PlayerName = x.PlayerPublicId == myPlayerId ? $"{x.DisplayName} (You)" : x.DisplayName,
              PlayerImageUrl = UserDataEditorViewModel.ToPlayerAvatarUrl(x.AvatarSeed),
              Bid = x.CurrentBid.HasValue ? x.CurrentBid.Value : double.NaN,
              Offer = x.CurrentAsk.HasValue ? x.CurrentAsk.Value : double.NaN,
              PositionPrice = x.PositionCashFlow.HasValue && x.PositionQty.HasValue && x.PositionQty != 0
                ? x.PositionCashFlow.Value / x.PositionQty.Value : double.NaN,
              PositionQty = x.PositionQty.HasValue ? x.PositionQty.Value : 0,
              SettlementPnl = x.SettlementPnl.HasValue ? x.SettlementPnl.Value : double.NaN,
              IsConnected = x.IsConnected,
              PlayerCardImageUrl = GetCardImageUrl(x.SettlementCardId ?? UnopenedCard.CardId)
            };

            if (!GameUpdateResponse.IsFinished)
            {
              if (data.PositionQty == 0)
              {
                data.CurrentPnl = (-x.PositionCashFlow ?? double.NaN);
              }
              else
              {
                data.CurrentPnl = data.PositionQty > 0 ? (BestBid - data.PositionPrice) * data.PositionQty
                : (data.PositionQty < 0 ? (BestAsk - data.PositionPrice) * data.PositionQty : double.NaN);
              }
            }
            return data;
          }).OrderBy(x => x.PlayerName);
        }
        return Enumerable.Empty<PlayerData>();
      }
    }

    public IEnumerable<TradeData> TradesData
    {
      get
      {
        if (GameUpdateResponse == null || Trades == null)
        {
          return Enumerable.Empty<TradeData>();
        }

        var names = GameUpdateResponse.PlayerPublicStates.ToDictionary(x => x.PlayerPublicId, x => x.DisplayName);
        return Trades.Values.OrderBy(x => -x.TradeId).Select((x, i) =>
        {
          var initiator = names.GetValueOrDefault(x.InitiatorPlayerPublicId) ?? x.InitiatorPlayerPublicId.ToString();
          var target = names.GetValueOrDefault(x.TargetPlayerPublicId) ?? x.TargetPlayerPublicId.ToString();
          return new TradeData()
          {
            TradeSequence = Trades.Count - i,
            BuyerName = x.IsBuy ? initiator : target,
            SellerName = x.IsBuy ? target : initiator,
            TradePrice = x.TradePrice,
            TradeQty = x.TradeQty
          };
        });
      }
    }

    public string PriceStyle(double? val)
    {
      if (val == null || !Double.IsFinite(val.Value) || val == 0)
      {
        return string.Empty;
      }
      else if (val > 0)
      {
        return "font-weight: bold; color: limegreen;";
      }
      else
      {
        return "font-weight: bold; color: darkred;";
      }
    }

    public string FormatPrice(double? val)
    {
      var ret = val == null || Double.IsNaN(val.Value) ? "-" : String.Format("${0:f2}", val.Value);
      return ret;
    }

    public string FormatQty(double? val)
    {
      return val == null || Double.IsNaN(val.Value) ? "-" : String.Format("{0:f2}", val.Value);
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

    public async Task SendUpdateQuoteRequest()
    {
      if (!CheckIsConnected("UpdateQuote"))
      {
        return;
      }

      if (!(Double.IsFinite(AskPrice) && Double.IsFinite(BidPrice)))
      {
        await JSRuntime.InvokeAsync<object>("alert", $"Are you sure you want to Update Quote?");
      }

      if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to Update Quote?"))
        return;

      await GameClient.SendRequestAsync("UpdateQuote", new UpdateQuoteRequest()
      {
        CurrentAsk = AskPrice,
        CurrentBid = BidPrice,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
    }

    public async Task SendTradeRequest(bool isBuy)
    {
      if (!CheckIsConnected("Trade"))
      {
        return;
      }

      if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to Submit Trade?"))
        return;

      await GameClient.SendRequestAsync("Trade", new TradeRequest()
      {
        IsBuy = isBuy,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
    }

    public async Task SendDealRequest()
    {
      var request = GameUpdateResponse.AllRoundsFinished ?
        DealGameRequest.RequestTypes.FinishGame : DealGameRequest.RequestTypes.DealCard;

      if (!CheckIsConnected(request.ToString()))
      {
        return;
      }

      if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to {request}?"))
        return;

      await GameClient.SendRequestAsync("DealGame", new DealGameRequest()
      {
        RequestType = request,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
    }

    public string LockTradingStatus
    {
      get
      {
        if (GameUpdateResponse != null)
        {
          return GameUpdateResponse.IsTradingLocked ? "Unlock Trading" : "Lock Trading";
        }
        return "";
      }
    }

    public async Task SendLockTradingRequest()
    {
      var block = !GameUpdateResponse?.IsTradingLocked ?? false;
      var request = block ? DealGameRequest.RequestTypes.LockTrading : DealGameRequest.RequestTypes.UnlockTrading;
      if (!CheckIsConnected(request.ToString()))
      {
        return;
      }

      await GameClient.SendRequestAsync("DealGame", new DealGameRequest()
      {
        RequestType = request,
        GameId = GameId,
        PlayerId = UserDataEditor.Data.PlayerId
      });
    }

    public void SetBidAskToJoin()
    {
      if (GameUpdateResponse != null)
      {
        BidPrice = GameUpdateResponse.BestCurrentBid ?? BidPrice;
        AskPrice = GameUpdateResponse.BestCurrentAsk ?? AskPrice;
        InvokeStateChanged(EventArgs.Empty);
      }
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