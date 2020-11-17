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

      return playerState; ;
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

      foreach (var playerState in playersToDeal)
      {
        var drawnCardId = _cardDeck.Draw();
        playerState.PlayerCardCardId = drawnCardId;
      }

      await _service.DBContext.SaveChangesAsync();
      return playersToDeal;
    }

    public async Task<bool> DealCommunityCard()
    {
      EnsureCardDeckInitialized();

      var numOfDoneRounds = GameState.RoundStates
        .Where(x => x.CommunityCardCardId != _service.UnopenedCard.CardId).Count();

      if (numOfDoneRounds < GameState.Game.NumberOfRounds)
      {
        
      }


      return false;
    }
  }
}