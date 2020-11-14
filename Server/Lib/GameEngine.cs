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

namespace MarketMakingGame.Server.Lib
{
  public class GameEngine
  {
    private ILogger _logger;
    private GameService _service;

    private GameState GameState { get; set; }

    public Game Game => GameState.Game;

    public GameEngine(ILoggerProvider loggerProvider, GameService service)
    {
      _logger = loggerProvider.CreateLogger(nameof(GameEngine));
      _service = service;
    }

    public async Task InitializeAsync(CreateGameRequest request)
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
        PlayerId = request.Player.PlayerId
      };

      var playerState = new PlayerState()
      {

      };

      _service.DBContext.GameStates.Add(GameState);
      await _service.DBContext.SaveChangesAsync();
      _logger.LogInformation("Created GameState={}", GameState);
    }

    public async Task JoinGameAsync(JoinGameRequest request)
    {
      
    }
  }
}