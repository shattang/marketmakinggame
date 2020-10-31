using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Lib
{
  public class GameEngine
  {
    public Dictionary<string, GameInfo> _gameInfos = new Dictionary<string, GameInfo>();

    //public static GameEngine Instance { get; } = new GameEngine();
  }
}