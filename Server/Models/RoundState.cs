using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Server.Models
{
  public class RoundState
  {
    public int RountStateId { get; set; }

    public int GameStateId { get; set; }
    public GameState GameState { get; set; }

    public string CommunityCardId { get; set; }
    
    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}