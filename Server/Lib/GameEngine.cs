using MarketMakingGame.Shared.Models;
using MarketMakingGame.Server.Models;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Server.Data;
using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Server.Lib
{
  public class GameEngine
  {
    private GameDbContext _dbContext;

    public Game Game { get; set; }

    private GameState GameState { get; set; }

    public GameEngine(CreateGameRequest request, GameDbContext dbContext)
    {
      Game = request.Game;
      Game.GameId = new Guid().ToBase62();
      _dbContext = dbContext;
      GameState = new GameState();
      
    }
  }
}