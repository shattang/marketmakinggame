using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public class TradeUpdate
  {
    public int InitiatingPlayerPlayerPublicId { get; set; }

    public int TargetPlayerPlayerPublicId { get; set; }

    public bool IsBuy { get; set; }

    public double TradePrice { get; set; }
  }
}