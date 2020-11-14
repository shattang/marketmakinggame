using MarketMakingGame.Shared.Models;
using MarketMakingGame.Server.Models;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Server.Data;
using System;
using MarketMakingGame.Shared.Lib;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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
      request.Game.GameId = new Guid().ToBase62();
      _service.DBContext.Games.Add(request.Game);

      if (_service.DBContext.Players.Find(request.Player.PlayerId) == null)
      {
        _service.DBContext.Players.Add(request.Player);
      }

      GameState = new GameState()
      {
        Game = request.Game,
        Player = request.Player
      };

      _service.DBContext.GameStates.Add(GameState);
      await _service.DBContext.SaveChangesAsync();
      _logger.LogInformation("Created GameId={}", GameState.Game.GameId);
    }

    public async Task JoinGameAsync(JoinGameRequest request)
    {

    }
  }
}