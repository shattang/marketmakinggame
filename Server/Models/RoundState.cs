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
    public GameState GameState { get; set; }

    public int CommunityCardCardId { get; set; }
    public Card CommunityCard { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}