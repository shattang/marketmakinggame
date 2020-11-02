using System;
using System.Collections.Generic;

namespace MarketMakingGame.Server.Models
{
  public class PlayerState
  {
    public int PlayerCard { get; set; }

    public double CurrentBid { get; set; }

    public double CurrentAsk { get; set; }
  }
}