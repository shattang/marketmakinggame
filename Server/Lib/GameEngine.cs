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

    private GameState GameState { get; set; }

    public Game Game => GameState.Game;

    public GameEngine(ILoggerProvider loggerProvider, GameService service, GameState gameState = null)
    {
      _logger = loggerProvider.CreateLogger(nameof(GameEngine));
      _service = service;
      GameState = gameState;
    }

    public async Task InitializeGameStateAsync(CreateGameRequest request)
    {
      request.Game.GameId = Guid.NewGuid().ToBase62();
      await _service.DBContext.Games.AddAsync(request.Game);

      var player = await _service.DBContext.Players.FindAsync(request.Player.PlayerId);
      if (player == null)
      {
        await _service.DBContext.Players.AddAsync(request.Player);
      }

      GameState = new GameState()
      {
        GameId = request.Game.GameId,
        PlayerId = request.Player.PlayerId,
        PlayerStates = new List<PlayerState>()
        {
          new PlayerState()
          {
            PlayerId = request.Player.PlayerId,
            PlayerCardCardId = _service.UnopenedCard.CardId
          }
        },
        RoundStates = new List<RoundState>(),
        Trades = new List<Trade>()
      };

      _service.DBContext.GameStates.Add(GameState);
      await _service.DBContext.SaveChangesAsync();

      _logger.LogInformation("Added GameState: GameStateId={}", GameState.GameStateId);
    }

    public async Task JoinGameAsync(JoinGameRequest request)
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
    }

    public async Task DealPlayerCards()
    {
      
    }
  }
}