using MarketMakingGame.Shared.Models;
using MarketMakingGame.Server.Models;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Server.Data;
using System;
using MarketMakingGame.Shared.Lib;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MarketMakingGame.Server.Lib
{
  public class GameEngine
  {
    private ILogger _logger;
    GameDbContext _dbContext;
    CardRepository _cardRepo;
    public GameState GameState { get; set; }

    public GameEngine(ILoggerProvider loggerProvider, GameDbContext dbContext, CardRepository repository)
    {
      _logger = loggerProvider.CreateLogger(nameof(GameEngine));
      _dbContext = dbContext;
      _cardRepo = repository;
    }

    public async Task<PlayerState> CreateGameAsync(CreateGameRequest request)
    {
      request.Game.MinQuoteWidth = Math.Max(request.Game.MinQuoteWidth ?? 0, 1);
      request.Game.MaxQuoteWidth = Math.Max(request.Game.MaxQuoteWidth ?? 0, 1);
      request.Game.NumberOfRounds = Math.Max(request.Game.NumberOfRounds ?? 0, 1);
      request.Game.TradeQty = Math.Max(request.Game.TradeQty ?? 0, 1);
      request.Game.GameId = Guid.NewGuid().ToBase62();
      await _dbContext.Games.AddAsync(request.Game);

      var player = await _dbContext.Players.FindAsync(request.Player.PlayerId);
      if (player == null)
      {
        await _dbContext.Players.AddAsync(request.Player);
      }
      else
      {
        player.AvatarSeed = request.Player.AvatarSeed;
        player.DisplayName = request.Player.DisplayName;
      }

      PlayerState playerState = new PlayerState()
      {
        PlayerId = request.Player.PlayerId,
        PlayerCardCardId = _cardRepo.UnopenedCard.CardId,
        IsConnected = true
      };

      GameState = new GameState()
      {
        GameId = request.Game.GameId,
        PlayerId = request.Player.PlayerId,
        PlayerStates = new List<PlayerState>()
        {
          playerState
        },
        RoundStates = new List<RoundState>(),
        Trades = new List<Trade>()
      };

      _dbContext.GameStates.Add(GameState);
      await _dbContext.SaveChangesAsync();

      _logger.LogInformation("Added GameState: GameStateId={}", GameState.GameStateId);
      return playerState;
    }

    public async Task<PlayerState> JoinGameAsync(JoinGameRequest request)
    {
      var player = await _dbContext.Players.FindAsync(request.Player.PlayerId);
      if (player == null)
      {
        await _dbContext.Players.AddAsync(request.Player);
      }
      else
      {
        player.AvatarSeed = request.Player.AvatarSeed;
        player.DisplayName = request.Player.DisplayName;
      }

      var playerState = GameState.PlayerStates
        .FirstOrDefault(x => x.Player.PlayerId == request.Player.PlayerId);

      if (playerState == null)
      {
        playerState = new PlayerState()
        {
          PlayerId = request.Player.PlayerId,
          PlayerCardCardId = _cardRepo.UnopenedCard.CardId,
          IsConnected = true
        };
        GameState.PlayerStates.Add(playerState);
        _logger.LogInformation("Added PlayerState: PlayerStateId={}", playerState.PlayerStateId);
      }
      else
      {
        playerState.IsConnected = true;
      }

      await _dbContext.SaveChangesAsync();
      return playerState;
    }

    public async Task<(bool, string, IEnumerable<PlayerState>)> DealPlayerCards()
    {
      if (GameState.RoundStates.Count >= GameState.Game.NumberOfRounds)
      {
        return (false, "Cannot deal, All rounds are done", Enumerable.Empty<PlayerState>());
      }

      var cardDeck = _cardRepo.GameStateToCardDeck.GetOrAdd(GameState.GameStateId, _ => {
        return new CardDeck(_cardRepo);
      });

      if (cardDeck.NeedsRebuild(GameState.CardDeckHash))
      {
        GameState.CardDeckHash = cardDeck.RebuildDeck(GameState);
      }

      var playersToDeal = GameState.PlayerStates
        .Where(x => x.PlayerCardCardId == _cardRepo.UnopenedCard.CardId).ToList();

      if (playersToDeal.Count > 0)
      {
        foreach (var playerState in playersToDeal)
        {
          var drawnCard = cardDeck.PickCard(GameState.CardDeckHash);
          GameState.CardDeckHash = drawnCard.Hash;
          playerState.PlayerCardCardId = drawnCard.CardId;
        }
      }
      else
      {
        var drawnCard = cardDeck.PickCard(GameState.CardDeckHash);
        GameState.CardDeckHash = drawnCard.Hash;
        var roundState = new RoundState()
        {
          CommunityCardCardId = drawnCard.CardId
        };
        GameState.RoundStates.Add(roundState);
      }

      await _dbContext.SaveChangesAsync();

      return (true, string.Empty, playersToDeal);
    }

    public async Task<UpdateQuoteResponse> UpdateQuote(UpdateQuoteRequest request)
    {
      var resp = new UpdateQuoteResponse() { RequestId = request.RequestId, Message = string.Empty };

      if (GameState.IsFinished)
      {
        resp.ErrorMessage = "Game is Finished";
        return resp;
      }

      var playerState = GameState.PlayerStates
        .FirstOrDefault(x => x.PlayerId == request.PlayerId);

      if (playerState == null)
      {
        resp.ErrorMessage = "PlayerId not found";
        return resp;
      }

      var newAsk = request.CurrentAsk ?? playerState.CurrentAsk;
      var newBid = request.CurrentBid ?? playerState.CurrentBid;

      if (!(newAsk.HasValue && newBid.HasValue))
      {
        resp.ErrorMessage = "Need to quote both Bid and Ask";
        return resp;
      }

      var currBestAsk = GameState.PlayerStates
        .Where(x => x.PlayerStateId != playerState.PlayerStateId).Select(x => x.CurrentAsk).Min();
      var currBestBid = GameState.PlayerStates
        .Where(x => x.PlayerStateId != playerState.PlayerStateId).Select(x => x.CurrentBid).Max();

      if (GameState.BestCurrentBid.HasValue && newAsk <= currBestBid)
      {
        newAsk = currBestBid + GameState.Game.MinQuoteWidth;
        resp.Message += $"Ask crossed Best Bid and was limited to {newAsk}. ";
      }

      if (GameState.BestCurrentAsk.HasValue && newBid >= currBestAsk)
      {
        newBid = currBestAsk - GameState.Game.MinQuoteWidth;
        resp.Message += $"Bid crossed Best Ask and was limited to {newBid}. ";
      }

      var quoteWidth = newAsk.Value - newBid.Value;
      if (quoteWidth > GameState.Game.MaxQuoteWidth)
      {
        resp.ErrorMessage = $"Quote width should be less than {GameState.Game.MaxQuoteWidth}";
        return resp;
      }

      if (quoteWidth < GameState.Game.MinQuoteWidth)
      {
        resp.ErrorMessage = $"Quote width should be less than {GameState.Game.MinQuoteWidth}";
        return resp;
      }

      playerState.CurrentAsk = newAsk;
      playerState.CurrentBid = newBid;

      GameState.BestCurrentAsk = GameState.PlayerStates.Select(x => x.CurrentAsk).Min();
      GameState.BestCurrentBid = GameState.PlayerStates.Select(x => x.CurrentBid).Max();

      await _dbContext.SaveChangesAsync();
      resp.BidPrice = newBid;
      resp.AskPrice = newAsk;
      resp.IsSuccess = true;
      return resp;
    }

    public async Task<(bool, string)> LockTrading(bool locked)
    {
      if (GameState.IsTradingLocked == locked)
      {
        return (false, "Trading is already " + (locked ? "Locked" : "Unlocked"));
      }

      GameState.IsTradingLocked = locked;
      await _dbContext.SaveChangesAsync();
      return (true, null);
    }

    public async Task<(bool, string, List<Trade>)> Trade(TradeRequest request)
    {
      if (GameState.IsTradingLocked)
      {
        return (false, "Trading is currently locked by Dealer", null);
      }

      if (GameState.IsFinished)
      {
        return (false, "Game is marked finished by Dealer", null);
      }

      var initiatorPlayer = GameState.PlayerStates
        .FirstOrDefault(x => x.PlayerId == request.PlayerId);

      if (initiatorPlayer == null)
      {
        return (false, "PlayerId not found", null);
      }

      List<PlayerState> targetPlayers;
      if (request.IsBuy)
      {
        if (!GameState.BestCurrentAsk.HasValue)
        {
          return (false, "No Best Ask Price Quoted to Buy", null);
        }

        targetPlayers = GameState.PlayerStates
          .Where(x => x.PlayerStateId != initiatorPlayer.PlayerStateId &&
            x.CurrentAsk.HasValue &&
            Math.Abs(x.CurrentAsk.Value - GameState.BestCurrentAsk.Value) < 1E-6)
          .ToList();

      }
      else
      {
        if (!GameState.BestCurrentBid.HasValue)
        {
          return (false, "No Best Bid Price Quoted to Sell", null);
        }

        targetPlayers = GameState.PlayerStates
          .Where(x => x.IsConnected)
          .Where(x => x.PlayerStateId != initiatorPlayer.PlayerStateId &&
            x.CurrentBid.HasValue &&
            Math.Abs(x.CurrentBid.Value - GameState.BestCurrentBid.Value) < 1E-6)
          .ToList();
      }

      if (targetPlayers.Count == 0)
      {
        if (initiatorPlayer.CurrentAsk.HasValue || initiatorPlayer.CurrentBid.HasValue)
        {
          return (false, $"Cannot Trade. You are the only one on Best {(request.IsBuy ? "Ask" : "Bid")}", null);
        }
        return (false, "No Players to Trade with", null);
      }

      var trades = new List<Trade>();
      var initiatorTradeQty = GameState.Game.TradeQty.Value;
      var tradePrice = request.IsBuy ? GameState.BestCurrentAsk.Value : GameState.BestCurrentBid.Value;
      var targetTradeQty = initiatorTradeQty / targetPlayers.Count;
      foreach (var targetPlayer in targetPlayers)
      {
        var trade = new Trade()
        {
          InitiatorPlayerStateId = initiatorPlayer.PlayerStateId,
          TargetPlayerStateId = targetPlayer.PlayerStateId,
          IsBuy = request.IsBuy,
          TradePrice = tradePrice,
          TradeQty = targetTradeQty
        };
        trades.Add(trade);
        GameState.Trades.Add(trade);

        var currTargetPosQty = targetPlayer.PositionQty ?? 0;
        var currTargetPosCashflow = targetPlayer.PositionCashFlow ?? 0;
        var targetSide = request.IsBuy ? -1 : 1;
        targetPlayer.PositionQty = currTargetPosQty + targetTradeQty * targetSide;
        targetPlayer.PositionCashFlow = currTargetPosCashflow + targetTradeQty * tradePrice * targetSide;

        targetPlayer.CurrentAsk = null;
        targetPlayer.CurrentBid = null;
      }

      var initiatorSide = request.IsBuy ? 1 : -1;
      var currPosQty = initiatorPlayer.PositionQty ?? 0;
      var currPosCashflow = initiatorPlayer.PositionCashFlow ?? 0;
      initiatorPlayer.PositionQty = currPosQty + initiatorTradeQty * initiatorSide;
      initiatorPlayer.PositionCashFlow = currPosCashflow + initiatorTradeQty * tradePrice * initiatorSide;

      GameState.BestCurrentAsk = GameState.PlayerStates.Select(x => x.CurrentAsk).Min();
      GameState.BestCurrentBid = GameState.PlayerStates.Select(x => x.CurrentBid).Max();

      await _dbContext.SaveChangesAsync();

      return (true, null, trades);
    }

    public async Task<(bool, string)> FinishGame()
    {
      if (GameState.IsFinished)
      {
        return (false, "Game is already finished");
      }

      var settlementPrice = GameState.RoundStates
        .Select(x => x.CommunityCard.CardValue)
        .Concat(GameState.PlayerStates.Select(x => x.PlayerCard.CardValue))
        .Sum();

      GameState.SettlementPrice = settlementPrice;

      foreach (var playerState in GameState.PlayerStates)
      {
        if (playerState.PositionQty.HasValue && Math.Abs(playerState.PositionQty.Value) > 1E-6)
        {
          var positionPrice = (playerState.PositionCashFlow ?? 0) / playerState.PositionQty.Value;
          playerState.SettlementPnl = (settlementPrice - positionPrice) * playerState.PositionQty.Value;
        }
        else
        {
          playerState.SettlementPnl = -(playerState.PositionCashFlow ?? 0);
        }
      }

      GameState.IsFinished = true;
      await _dbContext.SaveChangesAsync();
      return (true, null);
    }
  }
}