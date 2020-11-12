using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Lib
{
  public class GameEngine
  {
    public Game Game { get; set; }

    public GameEngine(Game game)
    {
      Game = game;
    }

  }
}