using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class Game
  {
    public String GameId { get; set; }

    public String GameName { get; set; }

    public int? NumberOfRounds { get; set; } = 5;

    public double? MinQuoteWidth { get; set; } = 1;

    public double? MaxQuoteWidth { get; set; } = 5;
    
    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}