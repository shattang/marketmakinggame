using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Models
{
  public class RoundState
  {
    public int RoundStateId { get; set; }

    public int GameStateId { get; set; }
    public virtual GameState GameState { get; set; }

    public int CommunityCardCardId { get; set; }
    public virtual Card CommunityCard { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}