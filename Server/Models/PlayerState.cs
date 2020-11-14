using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Models
{
  public class PlayerState
  {
    public int PlayerStateId { get; set; }

    public string PlayerId { get; set; }
    public Player Player { get; set; }

    public int GameStateId { get; set; }
    public GameState GameState { get; set; }

    public int PlayerCardCardId { get; set; }
    public Card PlayerCard { get; set; }

    public double? CurrentBid { get; set; }

    public double? CurrentAsk { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}