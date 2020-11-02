using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class GameInfo
  {
    public String GameInfoId { get; set; }

    public String GameName { get; set; }

    public int NumberOfDecks { get; set; }

    public int NumberOfRounds { get; set; }

    public double MinQuoteWidth { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}