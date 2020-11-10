using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Lib
{
  public class GameService
  {
    public Dictionary<string, Game> _Games = new Dictionary<string, Game>();

    //public static GameEngine Instance { get; } = new GameEngine();
  }
}