using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class GameInfo
  {
    public String GameId { get; set; }

    public String GameName { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}