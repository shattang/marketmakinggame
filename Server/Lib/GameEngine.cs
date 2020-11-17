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
    private GameService _service;
    private Bag<int> _cardDeck;

    public GameState GameState { get; set; }

    public GameEngine(ILoggerProvider loggerProvider, GameService service, GameState gameState = null)
    {
      _logger = loggerProvider.CreateLogger(nameof(GameEngine));
      _service = service;
      GameState = gameState;
    }

    public async Task<PlayerState> CreateGameAsync(CreateGameRequest request)
    {
      request.Game.MinQuoteWidth = Math.Max(request.Game.MinQuoteWidth ?? 0, 1);
      request.Game.MaxQuoteWidth = Math.Max(request.Game.MaxQuoteWidth ?? 0, 1);
      request.Game.NumberOfRounds = Math.Max(request.Game.NumberOfRounds ?? 0, 1);
      request.Game.TradeQty = Math.Max(request.Game.TradeQty ?? 0, 1);
      request.Game.GameId = Guid.NewGuid().ToBase62();
      await _service.DBContext.Games.AddAsync(request.Game);

      var player = await _service.DBContext.Players.FindAsync(request.Player.PlayerId);
      if (player == null)
      {
        await _service.DBContext.Players.AddAsync(request.Player);
      }

      PlayerState playerState = new PlayerState()
      {
        PlayerId = request.Player.PlayerId,
        PlayerCardCardId = _service.UnopenedCard.CardId
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

      _service.DBContext.GameStates.Add(GameState);
      await _service.DBContext.SaveChangesAsync();

      _logger.LogInformation("Added GameState: GameStateId={}", GameState.GameStateId);
      return playerState;
    }

    public async Task<PlayerState> JoinGameAsync(JoinGameRequest request)
    {
      var playerState = GameState.PlayerStates
      .FirstOrDefault(x => x.Player.PlayerId == request.Player.PlayerId);

      if (playerState == null)
      {
        playerState = new PlayerState()
        {
          PlayerId = request.Player.PlayerId,
          PlayerCardCardId = _service.UnopenedCard.CardId
        };
        GameState.PlayerStates.Add(playerState);

        await _service.DBContext.SaveChangesAsync();
        _logger.LogInformation("Added PlayerState: PlayerStateId={}", playerState.PlayerStateId);
      }

      return playerState;
    }

    private void EnsureCardDeckInitialized()
    {
      if (_cardDeck == null)
      {
        var minCardsNeeded = (GameState.Game.NumberOfRounds ?? 0) + GameState.PlayerStates.Count;
        var minDecksNeeded = Math.Max(minCardsNeeded, 1) / _service.Cards.Count + 1;
        var dealtCards = GameState.RoundStates
          .Select(x => x.CommunityCard)
          .Concat(GameState.PlayerStates.Select(x => x.PlayerCard))
          .Where(x => x != null && x != _service.UnopenedCard)
          .Select(x => x.CardId);
        _cardDeck = new Bag<int>(_service.Cards.Select(x => x.CardId), dealtCards,
          _service.UnopenedCard.CardId, minDecksNeeded);
        _logger.LogInformation($"Card Deck Initialized NumOfDecks={minDecksNeeded}");
      }
    }

    public async Task<List<PlayerState>> DealPlayerCards()
    {
      EnsureCardDeckInitialized();

      var playersToDeal = GameState.PlayerStates
        .Where(x => x.PlayerCardCardId == _service.UnopenedCard.CardId).ToList();

      if (playersToDeal.Count > 0)
      {
        foreach (var playerState in playersToDeal)
        {
          var drawnCardId = _cardDeck.Draw();
          playerState.PlayerCardCardId = drawnCardId;
        }

        await _service.DBContext.SaveChangesAsync();
      }

      return playersToDeal;
    }

    public async Task<(bool, string)> DealNextCommunityCard()
    {
      EnsureCardDeckInitialized();

      if (GameState.RoundStates.Count >= GameState.Game.NumberOfRounds)
      {
        return (false, "Cannot deal, All rounds are done");
      }

      var roundState = new RoundState()
      {
        CommunityCardCardId = _cardDeck.Draw()
      };

      GameState.RoundStates.Add(roundState);
      await _service.DBContext.SaveChangesAsync();
      return (true, null);
    }

    public async Task<(bool, string)> UpdateQuote(UpdateQuoteRequest request)
    {
      var playerState = GameState.PlayerStates
        .FirstOrDefault(x => x.PlayerId == request.PlayerId);

      if (playerState == null)
      {
        return (false, "PlayerId not found");
      }

      var newAsk = request.CurrentAsk ?? playerState.CurrentAsk;
      var newBid = request.CurrentBid ?? playerState.CurrentBid;

      if (!(newAsk.HasValue && newBid.HasValue))
      {
        return (false, "Need to quote both Bid and Ask");
      }

      var quoteWidth = newAsk.Value - newBid.Value;
      if (quoteWidth > GameState.Game.MaxQuoteWidth)
      {
        return (false, $"Quote width should be less than {GameState.Game.MaxQuoteWidth}");
      }

      if (quoteWidth < GameState.Game.MinQuoteWidth)
      {
        return (false, $"Quote width should be less than {GameState.Game.MinQuoteWidth}");
      }

      if (GameState.BestCurrentBid.HasValue && newAsk <= GameState.BestCurrentBid)
      {
        return (false, "Quote Ask Cannot Cross Best Bid");
      }

      if (GameState.BestCurrentAsk.HasValue && newBid >= GameState.BestCurrentAsk)
      {
        return (false, "Quote Bid Cannot Cross Best Ask");
      }

      playerState.CurrentAsk = newAsk;
      playerState.CurrentBid = newBid;

      GameState.BestCurrentAsk = GameState.PlayerStates.Select(x => x.CurrentAsk).Min();
      GameState.BestCurrentBid = GameState.PlayerStates.Select(x => x.CurrentBid).Max();

      await _service.DBContext.SaveChangesAsync();
      return (true, null);
    }

    public async Task<(bool, string)> LockTrading(bool locked)
    {
      if (GameState.IsTradingLocked == locked)
      {
        return (false, "Trading is already " + (locked ? "Locked" : "Unlocked"));
      }

      GameState.IsTradingLocked = locked;
      await _service.DBContext.SaveChangesAsync();
      return (true, null);
    }

    public async Task<(bool, string, List<Trade>)> Trade(TradeRequest request)
    {
      if (GameState.IsTradingLocked)
      {
        return (false, "Trading is currently locked by Dealer", null);
      }

      var initiatorPlayer = GameState.PlayerStates
        .FirstOrDefault(x => x.PlayerId == request.PlayerId);

      if (initiatorPlayer == null)
      {
        return (false, "PlayerId not found", null);
      }

      if (!(initiatorPlayer.CurrentAsk.HasValue && initiatorPlayer.CurrentBid.HasValue))
      {
        return (false, "Need to set a Quote in order to Trade", null);
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
          .Where(x => x.PlayerStateId != initiatorPlayer.PlayerStateId &&
            x.CurrentBid.HasValue &&
            Math.Abs(x.CurrentBid.Value - GameState.BestCurrentBid.Value) < 1E-6)
          .ToList();
      }

      if (targetPlayers.Count == 0)
      {
        return (false, "No Players to Trade with", null);
      }

      var trades = new List<Trade>();
      var initiatorTradeQty = GameState.Game.TradeQty.Value;
      var tradePrice = request.IsBuy ? GameState.BestCurrentAsk.Value : GameState.BestCurrentBid.Value;
      var targetTradeQty = initiatorTradeQty / targetPlayers.Count;
      foreach(var targetPlayer in targetPlayers)
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
      }

      var initiatorSide = request.IsBuy ? 1 : -1;
      var currPosQty = initiatorPlayer.PositionQty ?? 0;
      var currPosCashflow = initiatorPlayer.PositionCashFlow ?? 0;
      initiatorPlayer.PositionQty = currPosQty + initiatorTradeQty * initiatorSide;
      initiatorPlayer.PositionCashFlow = currPosCashflow + initiatorTradeQty * tradePrice * initiatorSide;
      await _service.DBContext.SaveChangesAsync();

      return (true, null, trades);
    }
  }
}